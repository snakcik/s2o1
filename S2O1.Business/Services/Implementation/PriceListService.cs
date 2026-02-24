using AutoMapper;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace S2O1.Business.Services.Implementation
{
    public class PriceListService : IPriceListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public PriceListService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
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

        public async Task<IEnumerable<PriceListDto>> GetAllAsync(string? status = null)
        {
            var canSeeDeleted = await CanSeeDeletedAsync();
            var query = _unitOfWork.Repository<PriceList>().Query();

            if (canSeeDeleted)
            {
                query = query.IgnoreQueryFilters()
                    .Include(p => p.Product)
                    .Include(p => p.Supplier);
                
                if (status == "passive") query = query.Where(x => x.IsDeleted);
                else if (status == "all") query = query.Where(x => true);
                else query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                query = query.Include(p => p.Product)
                    .Include(p => p.Supplier)
                    .Where(x => !x.IsDeleted);
            }

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<IEnumerable<PriceListDto>>(data);
        }

        public async Task<PriceListDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PriceList>().Query()
                .Include(p => p.Product)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (entity == null) return null;
            return _mapper.Map<PriceListDto>(entity);
        }

        public async Task<PriceListDto> CreateAsync(CreatePriceListDto dto)
        {
            if (dto.PurchasePrice < 0 || dto.SalePrice < 0)
                throw new System.ArgumentException("Prices cannot be negative.");

            var entity = _mapper.Map<PriceList>(dto);
            entity.IsActive = true;
            entity.CreateDate = System.DateTime.Now;
            
            await _unitOfWork.Repository<PriceList>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<PriceListDto> UpdateAsync(UpdatePriceListDto dto)
        {
            if (dto.PurchasePrice < 0 || dto.SalePrice < 0)
                throw new System.ArgumentException("Prices cannot be negative.");

            var entity = await _unitOfWork.Repository<PriceList>().GetByIdAsync(dto.Id);
            if (entity == null || entity.IsDeleted) return null;

            _mapper.Map(dto, entity);

            _unitOfWork.Repository<PriceList>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PriceList>().GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            entity.IsDeleted = true;
            
            _unitOfWork.Repository<PriceList>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
