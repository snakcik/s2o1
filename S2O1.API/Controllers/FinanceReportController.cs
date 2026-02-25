using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S2O1.DataAccess.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;
using S2O1.Domain.Enums;
using System.Collections.Generic;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceReportController : ControllerBase
    {
        private readonly S2O1DbContext _context;

        public FinanceReportController(S2O1DbContext context)
        {
            _context = context;
        }

        [HttpGet("user-performance")]
        [Filters.Permission("FinanceReport", "Read")]
        public async Task<IActionResult> GetUserPerformance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var usersQuery = _context.Users.Where(u => !u.IsDeleted);
            var offersQuery = _context.Offers.Where(o => !o.IsDeleted);
            var invoicesQuery = _context.Invoices.Where(i => !i.IsDeleted && i.OfferId != null);

            if (startDate.HasValue) offersQuery = offersQuery.Where(o => o.CreateDate >= startDate.Value.Date);
            if (endDate.HasValue) offersQuery = offersQuery.Where(o => o.CreateDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));

            var users = await usersQuery.ToListAsync();
            var offers = await offersQuery.ToListAsync();
            var invoices = await invoicesQuery.ToListAsync();

            var data = users.Select(u => new
            {
                UserId = u.Id,
                UserName = $"{u.UserFirstName} {u.UserLastName}",
                TotalOffersPrepared = offers.Count(o => o.CreatedByUserId == u.Id),
                TotalOffersVolume = offers.Where(o => o.CreatedByUserId == u.Id).Sum(o => o.TotalAmount),
                InvoicedOffersCount = offers.Count(o => o.CreatedByUserId == u.Id && o.Status == OfferStatus.Completed),
                PendingOffersCount = offers.Count(o => o.CreatedByUserId == u.Id && (o.Status == OfferStatus.Pending || o.Status == OfferStatus.Approved))
            }).Where(x => x.TotalOffersPrepared > 0).OrderByDescending(x => x.TotalOffersVolume).ToList();

            return Ok(data);
        }

        [HttpGet("profitability")]
        [Filters.Permission("FinanceReport", "Read")]
        public async Task<IActionResult> GetProfitability([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var invoiceItemsQuery = _context.InvoiceItems
                .Include(i => i.Invoice)
                .Include(i => i.Product)
                .ThenInclude(p => p.PriceLists)
                .Where(i => !i.IsDeleted && !i.Invoice.IsDeleted);

            if (startDate.HasValue) invoiceItemsQuery = invoiceItemsQuery.Where(i => i.Invoice.IssueDate >= startDate.Value.Date);
            if (endDate.HasValue) invoiceItemsQuery = invoiceItemsQuery.Where(i => i.Invoice.IssueDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));

            var invoiceItems = await invoiceItemsQuery.ToListAsync();

            var profitabilityData = invoiceItems
                .Where(i => i.Product != null)
                .GroupBy(i => new { i.ProductId, i.Product.ProductName })
                .Select(g =>
                {
                    var items = g.ToList();
                    var totalSoldQuantity = items.Sum(x => x.Quantity);
                    var totalRevenue = items.Sum(x => x.TotalPrice);
                    
                    decimal activePurchasePrice = items.FirstOrDefault()?.Product?.PriceLists?.FirstOrDefault(pl => pl.IsActivePrice)?.PurchasePrice ?? 0;
                    var totalCost = totalSoldQuantity * activePurchasePrice;
                    var totalProfit = totalRevenue - totalCost;

                    return new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        TotalSoldQuantity = totalSoldQuantity,
                        TotalRevenue = totalRevenue,
                        TotalCost = totalCost,
                        TotalProfit = totalProfit
                    };
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToList();

            return Ok(profitabilityData);
        }

        [HttpGet("daily-trend")]
        [Filters.Permission("FinanceReport", "Read")]
        public async Task<IActionResult> GetDailyTrend([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var invoiceItemsQuery = _context.InvoiceItems
                .Include(i => i.Invoice)
                .Include(i => i.Product)
                .ThenInclude(p => p.PriceLists)
                .Where(i => !i.IsDeleted && !i.Invoice.IsDeleted);

            if (startDate.HasValue) invoiceItemsQuery = invoiceItemsQuery.Where(i => i.Invoice.IssueDate >= startDate.Value.Date);
            if (endDate.HasValue) invoiceItemsQuery = invoiceItemsQuery.Where(i => i.Invoice.IssueDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));

            var invoiceItems = await invoiceItemsQuery.ToListAsync();

            var dailyData = invoiceItems
                .Where(i => i.Product != null)
                .GroupBy(i => i.Invoice.IssueDate.Date)
                .Select(g =>
                {
                    var items = g.ToList();
                    var revenue = items.Sum(x => x.TotalPrice);
                    
                    var cost = items.Sum(x => 
                        x.Quantity * (x.Product?.PriceLists?.FirstOrDefault(pl => pl.IsActivePrice)?.PurchasePrice ?? 0)
                    );
                    
                    return new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        TotalRevenue = revenue,
                        TotalCost = cost,
                        TotalProfit = revenue - cost
                    };
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Ok(dailyData);
        }

        [HttpGet("aging-inventory")]
        [Filters.Permission("FinanceReport", "Read")]
        public async Task<IActionResult> GetAgingInventory()
        {
            var productsQuery = _context.Products
                .Where(p => !p.IsDeleted && p.CurrentStock > 0);

            var products = await productsQuery.Select(p => new { p.Id, p.ProductName, p.CurrentStock, p.CreateDate }).ToListAsync();
            var productIds = products.Select(p => p.Id).ToList();

            var entryDates = await _context.StockMovements
                .Where(sm => !sm.IsDeleted && sm.MovementType == MovementType.Entry && productIds.Contains(sm.ProductId))
                .GroupBy(sm => sm.ProductId)
                .Select(g => new { ProductId = g.Key, MinDate = g.Min(sm => sm.MovementDate) })
                .ToDictionaryAsync(x => x.ProductId, x => x.MinDate);

            var agingData = products.Select(p =>
            {
                var oldestEntryDate = entryDates.ContainsKey(p.Id) ? entryDates[p.Id] : p.CreateDate;
                var daysInStock = (DateTime.Now - oldestEntryDate).Days;

                return new
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    CurrentStock = p.CurrentStock,
                    OldestEntryDate = oldestEntryDate.ToString("yyyy-MM-dd"),
                    DaysInStock = daysInStock
                };
            }).OrderByDescending(x => x.DaysInStock).ToList();

            return Ok(agingData);
        }

        [HttpGet("logistics-performance")]
        [Filters.Permission("Warehouse", "Read")] // or "FinanceReport"
        public async Task<IActionResult> GetLogisticsPerformance(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] int? delivererUserId,
            [FromQuery] string? invoiceNumber)
        {
            var invoiceQuery = _context.Invoices
                .Include(i => i.AssignedDelivererUser)
                .Where(i => !i.IsDeleted && i.Status != InvoiceStatus.Cancelled);

            if (startDate.HasValue) 
                invoiceQuery = invoiceQuery.Where(i => i.IssueDate >= startDate.Value.Date);
                
            if (endDate.HasValue) 
                invoiceQuery = invoiceQuery.Where(i => i.IssueDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
                
            if (delivererUserId.HasValue) 
                invoiceQuery = invoiceQuery.Where(i => i.AssignedDelivererUserId == delivererUserId.Value);

            if (!string.IsNullOrEmpty(invoiceNumber))
                invoiceQuery = invoiceQuery.Where(i => i.InvoiceNumber.Contains(invoiceNumber));

            var invoices = await invoiceQuery.OrderByDescending(i => i.IssueDate).ToListAsync();

            var reportData = invoices.Select(i => new
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                IssueDate = i.IssueDate.ToString("yyyy-MM-dd HH:mm"),
                DelivererName = i.AssignedDelivererUser != null 
                    ? $"{i.AssignedDelivererUser.UserFirstName} {i.AssignedDelivererUser.UserLastName}" 
                    : "Atanmadı",
                DelivererUserId = i.AssignedDelivererUserId,
                
                AssignedDate = i.WarehouseAssignedDate?.ToString("yyyy-MM-dd HH:mm"),
                CompletedDate = i.WarehouseCompletedDate?.ToString("yyyy-MM-dd HH:mm"),
                
                // Ne kadar sonra üzerine aldı? (Fatura Kesim - Depo Atanma)
                WaitTimeMinutes = i.WarehouseAssignedDate.HasValue 
                    ? Math.Round((i.WarehouseAssignedDate.Value - i.IssueDate).TotalMinutes, 1) 
                    : (double?)null,
                    
                // Hazırlanması ne kadar sürdü? (Depo Atanma - Tamamlanma)
                PreparationTimeMinutes = (i.WarehouseAssignedDate.HasValue && i.WarehouseCompletedDate.HasValue) 
                    ? Math.Round((i.WarehouseCompletedDate.Value - i.WarehouseAssignedDate.Value).TotalMinutes, 1) 
                    : (double?)null,
                
                Status = i.Status.ToString()
            }).ToList();

            return Ok(reportData);
        }

        [HttpGet("incomplete-deliveries")]
        [Filters.Permission("Warehouse", "Read")]
        public async Task<IActionResult> GetIncompleteDeliveries()
        {
            var invoices = await _context.Invoices
                .Include(i => i.AssignedDelivererUser)
                .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.PartiallyDelivered)
                .OrderByDescending(i => i.WarehouseIncompleteDate)
                .ToListAsync();

            var reportData = invoices.Select(i => new
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                IssueDate = i.IssueDate.ToString("yyyy-MM-dd HH:mm"),
                DelivererName = i.AssignedDelivererUser != null 
                    ? $"{i.AssignedDelivererUser.UserFirstName} {i.AssignedDelivererUser.UserLastName}" 
                    : "Atanmadı",
                
                IncompleteDate = i.WarehouseIncompleteDate?.ToString("yyyy-MM-dd HH:mm"),
                
                // Ne zamandır yarım bekliyor?
                WaitTimeMinutes = i.WarehouseIncompleteDate.HasValue 
                    ? Math.Round((DateTime.Now - i.WarehouseIncompleteDate.Value).TotalMinutes, 1) 
                    : 0
            }).ToList();

            return Ok(reportData);
        }

        [HttpGet("transfer-history")]
        [Filters.Permission("Warehouse", "Read")] // or FinanceReport
        public async Task<IActionResult> GetTransferHistory()
        {
            var logs = await _context.InvoiceStatusLogs
                .Include(l => l.Invoice)
                .Include(l => l.User)
                .OrderByDescending(l => l.LogDate)
                .Select(l => new 
                {
                    InvoiceNumber = l.Invoice.InvoiceNumber,
                    IssueDate = l.Invoice.IssueDate.ToString("yyyy-MM-dd HH:mm"),
                    LogDate = l.LogDate.ToString("yyyy-MM-dd HH:mm"),
                    Action = l.Action, // Assigned, Transferred, Unassigned vs
                    Status = l.Status,
                    UserName = l.User != null ? $"{l.User.UserFirstName} {l.User.UserLastName}" : "Sistem/Açığa Alındı",
                    PreparedItemsCount = l.PreparedItemsCount
                }).Take(100).ToListAsync();

            return Ok(logs);
        }
    }
}
