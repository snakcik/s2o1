using AutoMapper;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CompanyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CompanyDto>> GetAllAsync()
        {
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();
            var dtos = new List<CompanyDto>();
            foreach(var c in companies)
            {
                dtos.Add(new CompanyDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    AllowNegativeStock = c.AllowNegativeStock
                });
            }
            return dtos;
        }

        public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto)
        {
            var company = new Company
            {
                CompanyName = dto.CompanyName,
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
                AllowNegativeStock = company.AllowNegativeStock
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
