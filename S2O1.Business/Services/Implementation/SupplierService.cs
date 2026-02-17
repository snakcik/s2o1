using AutoMapper;
using S2O1.Business.DTOs.Business;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace S2O1.Business.Services.Implementation
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _unitOfWork.Repository<Supplier>().FindAsync(s => !s.IsDeleted);
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        public async Task<SupplierDto> GetByIdAsync(int id)
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(id);
            if (supplier == null || supplier.IsDeleted) return null;
            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = _mapper.Map<Supplier>(dto);
            await _unitOfWork.Repository<Supplier>().AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<SupplierDto> UpdateAsync(UpdateSupplierDto dto)
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(dto.Id);
            if (supplier == null || supplier.IsDeleted) return null;

            _mapper.Map(dto, supplier);
            _unitOfWork.Repository<Supplier>().Update(supplier);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(id);
            if (supplier == null || supplier.IsDeleted) return false;

            supplier.IsDeleted = true;
            _unitOfWork.Repository<Supplier>().Update(supplier);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
