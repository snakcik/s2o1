using AutoMapper;
using S2O1.Business.DTOs.Invoice;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using S2O1.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStockService _stockService;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IStockService stockService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _stockService = stockService;
        }

        public async Task<InvoiceDto> GetByIdAsync(int id)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(id);
            if (invoice == null) return null;
            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto)
        {
            // Transaction? StockService handles its own transaction, but here we have Invoice + Stock.
            // Nested transactions or single comprehensive one?
            // UnitOfWork supports nesting or reusing scope.
            // Let's create transaction here.
            
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = new Invoice
                {
                    InvoiceNumber = "INV-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    IssueDate = DateTime.Now,
                    DueDate = dto.DueDate,
                    PreparedByUserId = dto.PreparedByUserId,
                    Status = InvoiceStatus.Draft, // Start as Draft? Or Approved immediately?
                    // Assuming direct approval for simplicity based on user request "Create Invoice" implies effect.
                    // But usually Draft -> Approve flow.
                    // Let's assume Draft first.
                    Items = dto.Items.Select(i => new InvoiceItem 
                    {
                         ProductId = i.ProductId,
                         Quantity = i.Quantity,
                         UnitPrice = i.UnitPrice,
                         VatRate = i.VatRate,
                         TotalPrice = i.Quantity * i.UnitPrice // simple calculation
                    }).ToList()
                };

                // Calculate Totals
                invoice.GrandTotal = invoice.Items.Sum(i => i.TotalPrice);
                invoice.TaxTotal = invoice.GrandTotal * 0.18m; // Dummy tax logic

                await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync(); // Get ID

                // If created from Offer, link it
                if (dto.OfferId.HasValue)
                {
                    var offer = await _unitOfWork.Repository<Offer>().GetByIdAsync(dto.OfferId.Value);
                    if (offer != null)
                    {
                        invoice.OfferId = offer.Id;
                        _unitOfWork.Repository<Invoice>().Update(invoice);
                    }
                }

                await transaction.CommitAsync();
                return _mapper.Map<InvoiceDto>(invoice);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ApproveInvoiceAsync(int invoiceId, int approverUserId)
        {
             using var transaction = await _unitOfWork.BeginTransactionAsync();
             try
             {
                 var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
                 if (invoice == null) throw new Exception("Invoice not found");
                 
                 // Load Items
                 var items = await _unitOfWork.Repository<InvoiceItem>().FindAsync(i => i.InvoiceId == invoiceId);
                 invoice.Items = items.ToList();

                 if (invoice.Status == InvoiceStatus.Approved) return true;

                 // Decrease Stock for each item
                 foreach (var item in invoice.Items)
                 {
                     // Get Product to find Warehouse
                     var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                     
                     var moveDto = new StockMovementDto
                     {
                         ProductId = item.ProductId,
                         WarehouseId = product.WarehouseId.GetValueOrDefault(),
                         MovementType = MovementType.Exit, // Invoice = Sale = Exit
                         Quantity = item.Quantity,
                         UserId = approverUserId,
                         DocumentNo = invoice.InvoiceNumber,
                         Description = $"Invoice Approved",
                         CustomerId = invoice.BuyerCompanyId // Assuming Invoice has BuyerCompanyId filled
                     };

                     // StockService handles negative stock check and critical alerts
                     await _stockService.CreateMovementAsync(moveDto);
                 }

                 invoice.Status = InvoiceStatus.Approved;
                 invoice.ApprovedByUserId = approverUserId;
                 _unitOfWork.Repository<Invoice>().Update(invoice);
                 
                 await _unitOfWork.SaveChangesAsync();
                 await transaction.CommitAsync();
                 return true;
             }
             catch
             {
                 await transaction.RollbackAsync();
                 throw;
             }
        }
        public async Task<System.Collections.Generic.IEnumerable<InvoiceDto>> GetAllAsync()
        {
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync();
            return _mapper.Map<System.Collections.Generic.IEnumerable<InvoiceDto>>(invoices);
        }
    }
}
