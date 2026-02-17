using AutoMapper;
using S2O1.Business.DTOs.Invoice;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using S2O1.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IInvoiceService _invoiceService;

        public OfferService(IUnitOfWork unitOfWork, IMapper mapper, IInvoiceService invoiceService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _invoiceService = invoiceService;
        }

        public async Task ApproveOfferAsync(int offerId, int approverUserId)
        {
             using var transaction = await _unitOfWork.BeginTransactionAsync();
             try
             {
                 var offer = await _unitOfWork.Repository<Offer>().GetByIdAsync(offerId);
                 if (offer == null) throw new Exception("Offer not found");
                 
                 if (offer.Status != OfferStatus.Pending)
                     throw new Exception("Only pending offers can be approved.");

                 // Just update status. Stock will be decreased when Invoiced.
                 offer.Status = OfferStatus.Approved;
                 _unitOfWork.Repository<Offer>().Update(offer);

                 await _unitOfWork.SaveChangesAsync();
                 await transaction.CommitAsync();
             }
             catch
             {
                 await transaction.RollbackAsync();
                 throw;
             }
        }

        public async Task<int> CreateInvoiceFromOfferAsync(int offerId, int userId)
        {
            var offer = await _unitOfWork.Repository<Offer>().GetByIdAsync(offerId);
            if (offer == null) throw new Exception("Offer not found");
            
            if (offer.Status != OfferStatus.Approved)
                throw new Exception("Offer must be Approved before invoicing."); // Or allow creating invoice directly? Usually Approved.

            // Load Items
            var items = await _unitOfWork.Repository<OfferItem>().FindAsync(i => i.OfferId == offerId);
            
            var invoiceDto = new CreateInvoiceDto
            {
                OfferId = offerId,
                PreparedByUserId = userId,
                DueDate = DateTime.Now.AddDays(30), // Default term
                ReceiverCompanyId = offer.CustomerId, // Linking Customer to Receiver Company ID? Customer might not be Company ID. Mapping logic needed.
                // Assuming logic for Company ID resolution matches or Customer has CompanyId link.
                // For now, passing 0 or dummy if logic is complex.
                // Customer entity has CustomerCompanyId. Let's fetch Customer.
                Items = items.Select(i => new InvoiceItemDto 
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    VatRate = 18 // Default or from Product?
                }).ToList()
            };
            
            // Resolve Company ID from Customer
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(offer.CustomerId);
            if (customer != null)
            {
                invoiceDto.ReceiverCompanyId = customer.CustomerCompanyId;
            }

            var result = await _invoiceService.CreateAsync(invoiceDto);
            
            // Should we Auto-Approve invoice? Or leave as Draft?
            // "Tekliften faturaya dönüştürme" usually creates draft.
            // User can then approve invoice to deduct stock.
            
            return result.Id;
        }
        public async Task<IEnumerable<S2O1.Business.DTOs.Stock.OfferDto>> GetAllAsync()
        {
            var offers = await _unitOfWork.Repository<Offer>().Query()
                .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                .Include(x => x.Customer)
                .ThenInclude(c => c.CustomerCompany)
                .Where(x => !x.IsDeleted)
                .ToListAsync();
            return _mapper.Map<IEnumerable<S2O1.Business.DTOs.Stock.OfferDto>>(offers);
        }

        public async Task<S2O1.Business.DTOs.Stock.OfferDto> GetByIdAsync(int id)
        {
            var offer = await _unitOfWork.Repository<Offer>().Query()
                .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                .Include(x => x.Customer)
                .ThenInclude(c => c.CustomerCompany)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            
            return _mapper.Map<S2O1.Business.DTOs.Stock.OfferDto>(offer);
        }

        public async Task<S2O1.Business.DTOs.Stock.OfferDto> CreateAsync(S2O1.Business.DTOs.Stock.CreateOfferDto dto)
        {
            var offer = _mapper.Map<Offer>(dto);
            offer.OfferNumber = "TEK-" + DateTime.Now.Ticks.ToString().Substring(10);
            offer.OfferDate = DateTime.Now;
            offer.Status = OfferStatus.Pending;
            offer.IsActive = true;
            
            // Calculate Total
            offer.TotalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountRate / 100));

            await _unitOfWork.Repository<Offer>().AddAsync(offer);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(offer.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var offer = await _unitOfWork.Repository<Offer>().GetByIdAsync(id);
            if (offer == null) return false;

            offer.IsDeleted = true;
            _unitOfWork.Repository<Offer>().Update(offer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
