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

        public PriceListService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PriceListDto>> GetAllAsync()
        {
            var data = await _unitOfWork.Repository<PriceList>().Query()
                .Include(p => p.Product)
                .Include(p => p.Supplier)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

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
            var entity = _mapper.Map<PriceList>(dto);
            entity.IsActive = true;
            entity.CreateDate = System.DateTime.Now;
            
            await _unitOfWork.Repository<PriceList>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<PriceListDto> UpdateAsync(UpdatePriceListDto dto)
        {
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
