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
        private readonly ICurrentUserService _currentUserService;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IStockService stockService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _stockService = stockService;
            _currentUserService = currentUserService;
        }

        public async Task<InvoiceDto> GetByIdAsync(int id)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().Query()
                .Include(i => i.AssignedDelivererUser)
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Warehouse)
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Shelf)
                .Include(i => i.SellerCompany)
                .Include(i => i.BuyerCompany)
                .Include(i => i.Offer).ThenInclude(o => o.Customer).ThenInclude(c => c.CustomerCompany)
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (invoice == null) return null;
            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<InvoiceDto> GetByNumberAsync(string invoiceNumber)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().Query()
                .Include(i => i.AssignedDelivererUser)
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Warehouse)
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Shelf)
                .Include(i => i.SellerCompany)
                .Include(i => i.BuyerCompany)
                .Include(i => i.Offer).ThenInclude(o => o.Customer).ThenInclude(c => c.CustomerCompany)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
                
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
                var subTotal = invoice.Items.Sum(i => i.TotalPrice);
                invoice.TaxTotal = invoice.Items.Sum(i => i.TotalPrice * (i.VatRate / 100m));
                invoice.GrandTotal = subTotal + invoice.TaxTotal;

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
        public async Task<bool> RejectInvoiceAsync(int invoiceId)
        {
             using var transaction = await _unitOfWork.BeginTransactionAsync();
             try
             {
                 var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
                 if (invoice == null) throw new Exception("Invoice not found");
                 
                 invoice.Status = InvoiceStatus.Cancelled;
                 _unitOfWork.Repository<Invoice>().Update(invoice);

                 if (invoice.OfferId.HasValue)
                 {
                     var offer = await _unitOfWork.Repository<Offer>().GetByIdAsync(invoice.OfferId.Value);
                     if (offer != null)
                     {
                         offer.Status = OfferStatus.Pending; // Change back to Pending (Onaysız)
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

        private async Task<bool> CanSeeDeletedAsync()
        {
            if (_currentUserService.IsRoot) return true;
            return await _unitOfWork.Repository<UserPermission>().Query()
                .Include(p => p.Module)
                .AnyAsync(p => p.UserId == _currentUserService.UserId && 
                               p.Module.ModuleName == "ShowDeletedItems" && 
                               (p.CanRead || p.IsFull));
        }

        public async Task<S2O1.Business.DTOs.Common.PagedResultDto<InvoiceDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var canSeeDeleted = await CanSeeDeletedAsync();
            var query = _unitOfWork.Repository<Invoice>().Query();

            if (canSeeDeleted)
            {
                query = query.IgnoreQueryFilters()
                    .Include(i => i.AssignedDelivererUser)
                    .Include(i => i.Items).ThenInclude(it => it.Product)
                    .Include(i => i.SellerCompany)
                    .Include(i => i.BuyerCompany)
                    .Include(i => i.Offer).ThenInclude(o => o.Customer).ThenInclude(c => c.CustomerCompany);
                
                if (status == "passive")
                    query = query.Where(x => x.IsDeleted);
                else if (status == "all")
                    query = query.Where(x => true);
                else
                    query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                query = query.Include(i => i.AssignedDelivererUser)
                    .Include(i => i.Items).ThenInclude(it => it.Product)
                    .Include(i => i.SellerCompany)
                    .Include(i => i.BuyerCompany)
                    .Include(i => i.Offer).ThenInclude(o => o.Customer).ThenInclude(c => c.CustomerCompany)
                    .Where(x => !x.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(x => x.InvoiceNumber.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var invoices = await query.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);

            return new S2O1.Business.DTOs.Common.PagedResultDto<InvoiceDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<System.Collections.Generic.IEnumerable<InvoiceDto>> GetPendingDeliveriesAsync()
        {
            var invoices = await _unitOfWork.Repository<Invoice>().Query()
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Warehouse)
                .Include(i => i.Items).ThenInclude(it => it.Product).ThenInclude(p => p.Shelf)
                .Include(i => i.SellerCompany)
                .Include(i => i.BuyerCompany)
                .Include(i => i.Offer).ThenInclude(o => o.Customer).ThenInclude(c => c.CustomerCompany)
                .Where(i => i.Status == InvoiceStatus.WaitingForWarehouse || i.Status == InvoiceStatus.InPreparation || i.Status == InvoiceStatus.PartiallyDelivered)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
            
            return _mapper.Map<System.Collections.Generic.IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<bool> AssignToDelivererAsync(int invoiceId, int userId)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = InvoiceStatus.InPreparation;
            invoice.AssignedDelivererUserId = userId;
            invoice.WarehouseAssignedDate = DateTime.Now;
            
            _unitOfWork.Repository<Invoice>().Update(invoice);

            var log = new InvoiceStatusLog 
            {
                InvoiceId = invoiceId,
                UserId = userId,
                Action = "Assigned",
                Status = "InPreparation",
                LogDate = DateTime.Now
            };
            await _unitOfWork.Repository<InvoiceStatusLog>().AddAsync(log);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TransferJobAsync(int invoiceId, int toUserId)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
            if (invoice == null) return false;

            var oldUser = invoice.AssignedDelivererUserId;
            invoice.AssignedDelivererUserId = toUserId;
            // Biz burada süre ölçerken ilk alınma zamanını bozmamak için AssignedDate'i ezmiyoruz, loglardan devir vakitlerini analiz edeceğiz
            invoice.WarehouseIncompleteDate = DateTime.Now; // devredildiği an beklemeye düşmüş gibi sayılır/veya iz olarak kalır
            
            _unitOfWork.Repository<Invoice>().Update(invoice);

            var log = new InvoiceStatusLog 
            {
                InvoiceId = invoiceId,
                UserId = toUserId,
                Action = "Transferred From " + (oldUser?.ToString() ?? "Unknown"),
                Status = invoice.Status.ToString(),
                LogDate = DateTime.Now
            };
            await _unitOfWork.Repository<InvoiceStatusLog>().AddAsync(log);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnassignJobAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
            if (invoice == null) return false;

            var oldUser = invoice.AssignedDelivererUserId;
            invoice.Status = InvoiceStatus.WaitingForWarehouse;
            invoice.AssignedDelivererUserId = null;
            // İş tekrar havuza düştü. Assigndate sıfırlanabilir.
            invoice.WarehouseAssignedDate = null;
            invoice.WarehouseIncompleteDate = null;

            _unitOfWork.Repository<Invoice>().Update(invoice);

            var log = new InvoiceStatusLog 
            {
                InvoiceId = invoiceId,
                UserId = oldUser, // Eski kullanıcının bıraktığını raporlar
                Action = "Unassigned",
                Status = "WaitingForWarehouse",
                LogDate = DateTime.Now
            };
            await _unitOfWork.Repository<InvoiceStatusLog>().AddAsync(log);

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
                invoice.WarehouseCompletedDate = DateTime.Now;
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
                        if (!item.Product.WarehouseId.HasValue || item.Product.WarehouseId.Value == 0)
                        {
                            throw new Exception($"'{item.Product.ProductName}' ürünü için depoda herhangi bir konum belirlenmemiş (Depo bilgisi yok). Stok hareketini tamamlamak için lütfen ürün sayfasından ürüne bir depo ataması yapın.");
                        }

                        // 1. Stock Movement EXIT
                        var moveDto = new StockMovementDto
                        {
                            ProductId = item.ProductId,
                            WarehouseId = item.Product.WarehouseId.Value,
                            MovementType = MovementType.Exit,
                            Quantity = item.Quantity,
                            UserId = dto.DelivererUserId,
                            CustomerId = invoice.BuyerCompanyId,
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

        public async Task<bool> MarkAsIncompleteAsync(int invoiceId)
        {
            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = InvoiceStatus.PartiallyDelivered;
            invoice.WarehouseIncompleteDate = DateTime.Now;
            
            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
