using AutoMapper;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using S2O1.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace S2O1.Business.Services.Implementation
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StockService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private async Task CheckNegativeStockPermission(int userId, int productId, decimal quantityToRemove)
        {
            // 1. Get User's Company to check 'AllowNegativeStock' setting
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null || !user.CompanyId.HasValue) return; // Or throw? Assuming default false if no company logic? Let's proceed safe.

            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(user.CompanyId.Value);
            if (company == null) return;

            if (company.AllowNegativeStock) return; // Negative stock allowed, skip check.

            // 2. Check Product Stock
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null) return;

            if (product.CurrentStock < quantityToRemove)
            {
                throw new InvalidOperationException($"Insufficient stock for product '{product.ProductName}'. Current: {product.CurrentStock}, Requested: {quantityToRemove}. Company policy does not allow negative stock.");
            }
        }

        private async Task CheckCriticalStockLevel(int productId)
        {
            // Check if stock is below critical level after movement
            // We need to fetch StockAlerts for this product
            // Assuming StockAlert is linked to Product. Or we fetch via repository.
            
            var alerts = await _unitOfWork.Repository<StockAlert>().FindAsync(a => a.ProductId == productId);
            var alert = alerts.FirstOrDefault();
            
            if (alert != null)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
                if (product != null && product.CurrentStock <= alert.MinStockLevel)
                {
                    // Trigger Alert Logic
                    // For now, we update the alert status or log it.
                    // Ideally, create a Notification entity or send email.
                    // Requirement just says "integrate trigger".
                    
                    alert.IsNotificationSent = true; // Mark as needs notification or sent
                    _unitOfWork.Repository<StockAlert>().Update(alert);
                    
                    // TODO: Integrate with NotificationService if available
                }
            }
        }

        public async Task CreateMovementAsync(StockMovementDto movementDto)
        {
            // Transaction management
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (movementDto.Quantity < 0)
                    throw new ArgumentException("Quantity cannot be negative.");

                if (movementDto.MovementType == MovementType.Transfer) // Transfer implies Exit from Source
                {
                    // Check Negative Stock Permission for Source
                    await CheckNegativeStockPermission(movementDto.UserId, movementDto.ProductId, movementDto.Quantity);
                    await HandleTransferAsync(movementDto);
                }
                else if (movementDto.MovementType == MovementType.Exit)
                {
                    // Check Negative Stock Permission for Single Exit
                    await CheckNegativeStockPermission(movementDto.UserId, movementDto.ProductId, movementDto.Quantity);
                    await HandleSingleMovementAsync(movementDto);
                }
                else
                {
                    await HandleSingleMovementAsync(movementDto);
                }

                await _unitOfWork.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        }

        private async Task HandleSingleMovementAsync(StockMovementDto dto)
        {
            var movement = _mapper.Map<StockMovement>(dto);
            movement.MovementDate = DateTime.Now;
            
            await _unitOfWork.Repository<StockMovement>().AddAsync(movement);
            
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.ProductId);
            if (product == null) throw new Exception("Product not found");

            if (dto.MovementType == MovementType.Entry || dto.MovementType == MovementType.Return)
            {
                product.CurrentStock += dto.Quantity;
            }
            else // Exit
            {
                // Negative stock already checked in CreateMovementAsync wrapper
                product.CurrentStock -= dto.Quantity;
                
                // Check Critical Level after decrease
                await CheckCriticalStockLevel(product.Id);
            }
            
            _unitOfWork.Repository<Product>().Update(product);
        }

        private async Task HandleTransferAsync(StockMovementDto dto)
        {
            if (!dto.TargetWarehouseId.HasValue) 
                throw new ArgumentException("Target Warehouse is required for Transfer.");
            
            if (dto.WarehouseId == dto.TargetWarehouseId.Value)
                throw new ArgumentException("Source and Target Warehouse cannot be the same.");

            // 1. Get Source Product
            var sourceProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.ProductId);
            if (sourceProduct == null) throw new Exception("Source Product not found");

            // Negative stock checked in wrapper

            // Create Exit Movement
            var exitMovement = new StockMovement
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                TargetWarehouseId = dto.TargetWarehouseId,
                MovementType = MovementType.Exit, // Recorded as Exit from Source
                Quantity = dto.Quantity,
                MovementDate = DateTime.Now,
                UserId = dto.UserId,
                Description = $"Transfer Out to Warehouse {dto.TargetWarehouseId}. {dto.Description}",
                DocumentNo = dto.DocumentNo ?? "TRF-" + Guid.NewGuid().ToString().Substring(0,8)
            };
            
            sourceProduct.CurrentStock -= dto.Quantity;
            _unitOfWork.Repository<Product>().Update(sourceProduct);
            await _unitOfWork.Repository<StockMovement>().AddAsync(exitMovement);
            
            // Check Critical Level for Source Product
            await CheckCriticalStockLevel(sourceProduct.Id);

            // 3. Logic for Target (Entry)
            var targetProductList = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.ProductCode == sourceProduct.ProductCode && p.WarehouseId == dto.TargetWarehouseId.Value);
            
            var targetProduct = targetProductList.FirstOrDefault();

            if (targetProduct == null)
            {
                // Create Target Product (Clone from Source)
                targetProduct = new Product
                {
                    ProductName = sourceProduct.ProductName,
                    ProductCode = sourceProduct.ProductCode, // Same Code
                    CategoryId = sourceProduct.CategoryId,
                    BrandId = sourceProduct.BrandId,
                    UnitId = sourceProduct.UnitId,
                    WarehouseId = dto.TargetWarehouseId.Value,
                    LocationId = null, // Logic for default location?
                    CurrentStock = 0,
                    IsActive = true,
                    //CreateDate = DateTime.Now // BaseEntity creates this?
                };
                
                await _unitOfWork.Repository<Product>().AddAsync(targetProduct);
                await _unitOfWork.SaveChangesAsync(); // Need ID
            }

            // Create Entry Movement
            var entryMovement = new StockMovement
            {
                ProductId = targetProduct.Id, // The NEW or EXISTING product in target warehouse
                WarehouseId = dto.TargetWarehouseId.Value,
                TargetWarehouseId = dto.WarehouseId, // From Source
                MovementType = MovementType.Entry, // Recorded as Entry to Target
                Quantity = dto.Quantity,
                MovementDate = DateTime.Now,
                UserId = dto.UserId,
                Description = $"Transfer In from Warehouse {dto.WarehouseId}. {dto.Description}",
                DocumentNo = exitMovement.DocumentNo // Link by DocNo
            };

            targetProduct.CurrentStock += dto.Quantity;
            _unitOfWork.Repository<Product>().Update(targetProduct);
            await _unitOfWork.Repository<StockMovement>().AddAsync(entryMovement);
        }

        public async Task<decimal> GetProductStockAsync(int productId, int warehouseId)
        {
             var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
             if (product != null && product.WarehouseId == warehouseId)
             {
                 return product.CurrentStock;
             }
             return 0;
        }

        public async Task<IEnumerable<WarehouseStockReportDto>> GetWarehouseStockReportAsync(int? warehouseId)
        {
            var query = _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Warehouse)
                .Include(p => p.Unit)
                .Where(p => p.IsActive && !p.IsDeleted) // Only active products
                .AsNoTracking();

            if (warehouseId.HasValue)
            {
                var idValue = warehouseId.Value;
                query = query.Where(p => p.WarehouseId == idValue);
            }

            var products = await query.ToListAsync();
            var productIds = products.Select(p => p.Id).ToList();

            // FIXED: Added IsDeleted and IsActive checks to ensure deleted invoices don't reserve stock.
            var invoiceItemsQuery = await _unitOfWork.Repository<InvoiceItem>().Query()
                .Include(ii => ii.Invoice)
                .Where(ii => productIds.Contains(ii.ProductId) && 
                             !ii.IsDeleted && ii.IsActive &&              // Item must be active
                             !ii.Invoice.IsDeleted && ii.Invoice.IsActive && // Invoice must be active
                             (ii.Invoice.Status == InvoiceStatus.Approved || 
                              ii.Invoice.Status == InvoiceStatus.WaitingForWarehouse || 
                              ii.Invoice.Status == InvoiceStatus.InPreparation))
                .AsNoTracking()
                .ToListAsync();

            // FIXED: Also include Approved Offers that haven't been invoiced yet.
            var offerItemsQuery = await _unitOfWork.Repository<OfferItem>().Query()
                .Include(oi => oi.Offer)
                .Where(oi => productIds.Contains(oi.ProductId) &&
                             !oi.IsDeleted && oi.IsActive &&
                             !oi.Offer.IsDeleted && oi.Offer.IsActive &&
                             oi.Offer.Status == OfferStatus.Approved) // Only Approved ones reserve stock
                .AsNoTracking()
                .ToListAsync();

            var reservedByProductId = invoiceItemsQuery
                .Where(ii => ii.Invoice.Status == InvoiceStatus.Approved || ii.Invoice.Status == InvoiceStatus.WaitingForWarehouse)
                .GroupBy(ii => ii.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(ii => ii.Quantity));

            // Add OfferItem reservations to the dictionary
            foreach(var group in offerItemsQuery.GroupBy(oi => oi.ProductId))
            {
                if (reservedByProductId.ContainsKey(group.Key))
                    reservedByProductId[group.Key] += group.Sum(oi => oi.Quantity);
                else
                    reservedByProductId[group.Key] = group.Sum(oi => oi.Quantity);
            }

            var waitingByProductId = invoiceItemsQuery
                .Where(ii => ii.Invoice.Status == InvoiceStatus.InPreparation)
                .GroupBy(ii => ii.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(ii => ii.Quantity));

            return products.Select(p => 
            {
                decimal reserved = reservedByProductId.ContainsKey(p.Id) ? reservedByProductId[p.Id] : 0;
                decimal waiting = waitingByProductId.ContainsKey(p.Id) ? waitingByProductId[p.Id] : 0;
                
                return new WarehouseStockReportDto
                {
                    WarehouseName = p.Warehouse?.WarehouseName ?? "Depo Belirtilmemiş",
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    CurrentStock = p.CurrentStock,
                    ReservedStock = reserved,
                    WaitingInWarehouseStock = waiting,
                    AvailableStock = p.CurrentStock - reserved - waiting,
                    UnitName = p.Unit?.UnitName ?? "Birim Belirtilmemiş"
                };
            });
        }

        public async Task<IEnumerable<WaybillDto>> GetWaybillsBySupplierAsync(int supplierId)
        {
            return await SearchWaybillsAsync(null, null, null, supplierId, null);
        }

        public async Task<IEnumerable<WaybillDto>> SearchWaybillsAsync(string? waybillNo, DateTime? startDate, DateTime? endDate, int? supplierId, string? type)
        {
            var query = _unitOfWork.Repository<StockMovement>().Query()
                .Include(m => m.Supplier)
                .Include(m => m.Customer)
                .AsQueryable();

            if (type == "Giden")
            {
                query = query.Where(m => m.MovementType == MovementType.Exit);
            }
            else // Default to Gelen if not specified or specified as Gelen
            {
                query = query.Where(m => m.MovementType == MovementType.Entry);
            }

            if (!string.IsNullOrEmpty(waybillNo))
            {
                query = query.Where(m => m.DocumentNo.Contains(waybillNo));
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var nextDay = endDate.Value.AddDays(1);
                query = query.Where(m => m.MovementDate < nextDay);
            }

            if (supplierId.HasValue && supplierId > 0)
            {
                if (type == "Giden")
                {
                    query = query.Where(m => m.CustomerId == supplierId.Value);
                }
                else
                {
                    query = query.Where(m => m.SupplierId == supplierId.Value);
                }
            }

            var movements = await query.OrderByDescending(m => m.MovementDate).ToListAsync();

            // Group by DocumentNo to show unique waybills
            return movements.GroupBy(m => m.DocumentNo)
                .Select(g => new WaybillDto
                {
                    WaybillNo = g.Key ?? "Dökümansız",
                    Date = g.Max(m => m.MovementDate),
                    SupplierName = g.First().MovementType == MovementType.Exit ? 
                        (g.First().Customer?.CustomerContactPersonName ?? "Bilinmiyor") : 
                        (g.First().Supplier?.SupplierCompanyName ?? "Bilinmiyor"),
                    Description = g.First().Description,
                    DocumentPath = g.First().DocumentPath,
                    TotalQuantity = g.Sum(m => m.Quantity)
                });
        }

        public async Task<IEnumerable<WaybillItemDto>> GetWaybillItemsAsync(string waybillNo)
        {
            // Handle "Dökümansız" as null search
            bool isNullSearch = waybillNo == "Dökümansız" || string.IsNullOrEmpty(waybillNo);

            var query = _unitOfWork.Repository<StockMovement>().Query()
                .Where(m => isNullSearch ? m.DocumentNo == null : m.DocumentNo == waybillNo);

            return await query.Select(m => new WaybillItemDto
            {
                ProductId = m.ProductId,
                ProductCode = m.Product != null ? m.Product.ProductCode : "-",
                ProductName = m.Product != null ? m.Product.ProductName : "Bilinmeyen Ürün",
                Quantity = m.Quantity,
                UnitName = (m.Product != null && m.Product.Unit != null) ? m.Product.Unit.UnitName : "-",
                Description = m.Description
            }).ToListAsync();
        }
    }
}
