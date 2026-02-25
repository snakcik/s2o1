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
        private readonly ICurrentUserService _currentUserService;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
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

        public async Task<S2O1.Business.DTOs.Common.PagedResultDto<SupplierDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var canSeeDeleted = await CanSeeDeletedAsync();
            var query = _unitOfWork.Repository<Supplier>().Query();

            if (canSeeDeleted)
            {
                query = query.IgnoreQueryFilters();
                if (status == "passive") query = query.Where(x => x.IsDeleted);
                else if (status == "all") query = query.Where(x => true);
                else query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                if (status == "passive") return new S2O1.Business.DTOs.Common.PagedResultDto<SupplierDto>();
                query = query.Where(x => !x.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(x => x.SupplierCompanyName.ToLower().Contains(search) || 
                                         x.SupplierContactName.ToLower().Contains(search) ||
                                         x.SupplierContactMail.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var suppliers = await query.OrderByDescending(x => x.Id)
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();
                                       
            var mapped = _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
            return new S2O1.Business.DTOs.Common.PagedResultDto<SupplierDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
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
