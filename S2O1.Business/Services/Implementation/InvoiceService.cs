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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
                    }).ToList(),
                    SellerCompanyId = dto.SenderCompanyId,
                    BuyerCompanyId = dto.ReceiverCompanyId
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
                 var invoice = await _unitOfWork.Repository<Invoice>().Query()
                    .Include(i => i.Items).ThenInclude(it => it.Product)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                 if (invoice == null) throw new Exception("Invoice not found");
                 
                 if (invoice.Status == InvoiceStatus.Approved || invoice.Status == InvoiceStatus.WaitingForWarehouse) return true;

                 // Check if it has physical products
                 bool hasPhysical = invoice.Items.Any(i => i.Product != null && i.Product.IsPhysical);

                 if (hasPhysical)
                 {
                     invoice.Status = InvoiceStatus.WaitingForWarehouse;
                 }
                 else
                 {
                     invoice.Status = InvoiceStatus.Approved;
                 }

                 invoice.ApprovedByUserId = approverUserId;
                 _unitOfWork.Repository<Invoice>().Update(invoice);

                 // If linked to an offer, mark it as Completed
                 if (invoice.OfferId.HasValue)
                 {
                     var offer = await _unitOfWork.Repository<Offer>().GetByIdAsync(invoice.OfferId.Value);
                     if (offer != null)
                     {
                         offer.Status = OfferStatus.Completed;
                         _unitOfWork.Repository<Offer>().Update(offer);
                     }
                 }
                 
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
            var invoices = await _unitOfWork.Repository<Invoice>().Query()
                .Include(i => i.AssignedDelivererUser)
                .Include(i => i.Items).ThenInclude(it => it.Product)
                .ToListAsync();
            return _mapper.Map<System.Collections.Generic.IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<System.Collections.Generic.IEnumerable<InvoiceDto>> GetPendingDeliveriesAsync()
        {
            var invoices = await _unitOfWork.Repository<Invoice>().Query()
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Warehouse)
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Shelf)
                .Where(i => i.Status == InvoiceStatus.WaitingForWarehouse || i.Status == InvoiceStatus.InPreparation)
                .ToListAsync();
            
            return _mapper.Map<System.Collections.Generic.IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<bool> AssignToDelivererAsync(int invoiceId, int userId)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = InvoiceStatus.InPreparation;
            invoice.AssignedDelivererUserId = userId;
            
            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteDeliveryAsync(WarehouseDeliveryDto dto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.Repository<Invoice>().Query()
                    .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Brand)
                    .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Unit)
                    .Include(i => i.SellerCompany)
                    .Include(i => i.BuyerCompany)
                    .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

                if (invoice == null) throw new Exception("Invoice not found");

                invoice.Status = InvoiceStatus.Delivered;
                invoice.ReceiverName = dto.ReceiverName;
                invoice.AssignedDelivererUserId = dto.DelivererUserId;
                _unitOfWork.Repository<Invoice>().Update(invoice);

                // Create Dispatch Note (İrsaliye)
                var dispatchNote = new DispatchNote
                {
                    DispatchNo = "DN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    DispatchDate = DateTime.Now,
                    CompanyId = invoice.SellerCompanyId,
                    DelivererUserId = dto.DelivererUserId,
                    DelivererName = (await _unitOfWork.Repository<User>().GetByIdAsync(dto.DelivererUserId))?.UserFirstName + " " + (await _unitOfWork.Repository<User>().GetByIdAsync(dto.DelivererUserId))?.UserLastName,
                    ReceiverName = dto.ReceiverName,
                    Status = "Teslim Edildi",
                    Note = $"Produced from Invoice {invoice.InvoiceNumber}",
                    Items = new List<DispatchNoteItem>()
                };

                foreach (var item in invoice.Items)
                {
                    // Update include flag
                    item.IncludeInDispatch = dto.IncludedItemIds.Contains(item.Id);
                    _unitOfWork.Repository<InvoiceItem>().Update(item);

                    if (item.Product != null && item.Product.IsPhysical)
                    {
                        // 1. Stock Movement EXIT
                        var moveDto = new StockMovementDto
                        {
                            ProductId = item.ProductId,
                            WarehouseId = item.Product.WarehouseId.GetValueOrDefault(),
                            MovementType = MovementType.Exit,
                            Quantity = item.Quantity,
                            UserId = dto.DelivererUserId,
                            DocumentNo = invoice.InvoiceNumber,
                            Description = $"Warehouse Delivery Completed",
                        };
                        await _stockService.CreateMovementAsync(moveDto);

                        // 2. Add to Dispatch Note if requested
                        if (item.IncludeInDispatch)
                        {
                            dispatchNote.Items.Add(new DispatchNoteItem
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                UnitName = item.Product.Unit?.UnitShortName ?? "Adet"
                            });
                        }
                    }
                }

                await _unitOfWork.Repository<DispatchNote>().AddAsync(dispatchNote);
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
    }
}
