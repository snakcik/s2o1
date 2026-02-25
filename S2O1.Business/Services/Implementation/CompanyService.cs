using AutoMapper;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace S2O1.Business.Services.Implementation
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CompanyService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
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

        public async Task<S2O1.Business.DTOs.Common.PagedResultDto<CompanyDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var canSeeDeleted = await CanSeeDeletedAsync();
            var query = _unitOfWork.Repository<Company>().Query();

            if (canSeeDeleted)
            {
                query = query.IgnoreQueryFilters();
                if (status == "passive") query = query.Where(x => x.IsDeleted);
                else if (status == "all") query = query.Where(x => true);
                else query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                if (status == "passive") return new S2O1.Business.DTOs.Common.PagedResultDto<CompanyDto>();
                query = query.Where(x => !x.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(x => x.CompanyName.ToLower().Contains(search) || (x.TaxNumber != null && x.TaxNumber.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();
            var companies = await query.OrderByDescending(x => x.Id)
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();
            var dtos = new List<CompanyDto>();
            foreach(var c in companies)
            {
                dtos.Add(new CompanyDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    TaxNumber = c.TaxNumber,
                    Address = c.Address,
                    AllowNegativeStock = c.AllowNegativeStock,
                    IsDeleted = c.IsDeleted
                });
            }
            return new S2O1.Business.DTOs.Common.PagedResultDto<CompanyDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<CompanyDto> GetByIdAsync(int id)
        {
            var c = await _unitOfWork.Repository<Company>().GetByIdAsync(id);
            if (c == null) return null;
            return new CompanyDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                TaxNumber = c.TaxNumber,
                Address = c.Address,
                AllowNegativeStock = c.AllowNegativeStock,
                IsDeleted = c.IsDeleted
            };
        }

        public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto)
        {
            var company = new Company
            {
                CompanyName = dto.CompanyName,
                TaxNumber = dto.TaxNumber,
                Address = dto.Address,
                AllowNegativeStock = dto.AllowNegativeStock,
                IsActive = true,
                CreateDate = System.DateTime.Now
            };

            await _unitOfWork.Repository<Company>().AddAsync(company);
            await _unitOfWork.SaveChangesAsync();

            return new CompanyDto
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                TaxNumber = company.TaxNumber,
                Address = company.Address,
                AllowNegativeStock = company.AllowNegativeStock,
                IsDeleted = company.IsDeleted
            };
        }

        public async Task<CompanyDto> UpdateAsync(int id, CreateCompanyDto dto)
        {
            var c = await _unitOfWork.Repository<Company>().GetByIdAsync(id);
            if (c == null) throw new System.Exception("Şirket bulunamadı.");

            c.CompanyName = dto.CompanyName;
            c.TaxNumber = dto.TaxNumber;
            c.Address = dto.Address;
            c.AllowNegativeStock = dto.AllowNegativeStock;

            await _unitOfWork.SaveChangesAsync();

            return new CompanyDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                TaxNumber = c.TaxNumber,
                Address = c.Address,
                AllowNegativeStock = c.AllowNegativeStock,
                IsDeleted = c.IsDeleted
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(id);
            if (company == null) return false;

            // Check if users exist?
            // Usually soft delete.
            company.IsDeleted = true;
            company.IsActive = false;
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
