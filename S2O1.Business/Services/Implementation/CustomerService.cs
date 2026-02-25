using AutoMapper;
using Microsoft.EntityFrameworkCore;
using S2O1.Business.DTOs.Business;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
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

        // --- Customer Company Methods ---

        public async Task<S2O1.Business.DTOs.Common.PagedResultDto<CustomerCompanyDto>> GetAllCompaniesAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Repository<CustomerCompany>().Query();
            if (await CanSeeDeletedAsync())
            {
                query = query.IgnoreQueryFilters();
                if (status == "passive") query = query.Where(x => x.IsDeleted);
                else if (status == "all") query = query.Where(x => true);
                else query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                query = query.Where(x => !x.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(x => x.CustomerCompanyName.ToLower().Contains(search) || 
                                         x.CustomerCompanyAddress.ToLower().Contains(search) ||
                                         x.CustomerCompanyMail.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<CustomerCompanyDto>>(data);

            return new S2O1.Business.DTOs.Common.PagedResultDto<CustomerCompanyDto>
            {
                Items = mappedData,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<CustomerCompanyDto> GetCompanyByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<CustomerCompany>().Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return null;
            return _mapper.Map<CustomerCompanyDto>(entity);
        }

        public async Task<CustomerCompanyDto> CreateCompanyAsync(CreateCustomerCompanyDto dto)
        {
            var entity = _mapper.Map<CustomerCompany>(dto);
            entity.CreateDate = System.DateTime.Now;
            entity.IsActive = true;
            
            await _unitOfWork.Repository<CustomerCompany>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CustomerCompanyDto>(entity);
        }

        public async Task<CustomerCompanyDto> UpdateCompanyAsync(UpdateCustomerCompanyDto dto)
        {
            var entity = await _unitOfWork.Repository<CustomerCompany>().Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            _unitOfWork.Repository<CustomerCompany>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CustomerCompanyDto>(entity);
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var entity = await _unitOfWork.Repository<CustomerCompany>().Query().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            _unitOfWork.Repository<CustomerCompany>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // --- Customer Methods ---

        public async Task<S2O1.Business.DTOs.Common.PagedResultDto<CustomerDto>> GetAllCustomersAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var canSeeDeleted = await CanSeeDeletedAsync();
            var query = _unitOfWork.Repository<Customer>().Query();

            if (canSeeDeleted)
            {
                query = query.IgnoreQueryFilters().Include(x => x.CustomerCompany);
                if (status == "passive") query = query.Where(x => x.IsDeleted);
                else if (status == "all") query = query.Where(x => true);
                else query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                query = query.Include(x => x.CustomerCompany).Where(x => !x.IsDeleted);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(x => x.CustomerContactPersonName.ToLower().Contains(search) || 
                                         x.CustomerContactPersonLastName.ToLower().Contains(search) ||
                                         x.CustomerContactPersonMobilPhone.ToLower().Contains(search) || 
                                         x.CustomerContactPersonMail.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<CustomerDto>>(data);

            return new S2O1.Business.DTOs.Common.PagedResultDto<CustomerDto>
            {
                Items = mappedData,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Customer>().Query()
                .Include(x => x.CustomerCompany)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (entity == null) return null;
            return _mapper.Map<CustomerDto>(entity);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            var entity = _mapper.Map<Customer>(dto);
            entity.CreateDate = System.DateTime.Now;
            entity.IsActive = true;

            await _unitOfWork.Repository<Customer>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return await GetCustomerByIdAsync(entity.Id);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerDto dto)
        {
            var entity = await _unitOfWork.Repository<Customer>().GetByIdAsync(dto.Id);
            if (entity == null || entity.IsDeleted) return null;

            _mapper.Map(dto, entity);
            _unitOfWork.Repository<Customer>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return await GetCustomerByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Customer>().GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            entity.IsDeleted = true;
            _unitOfWork.Repository<Customer>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
