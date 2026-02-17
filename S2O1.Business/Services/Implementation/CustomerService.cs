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

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // --- Customer Company Methods ---

        public async Task<IEnumerable<CustomerCompanyDto>> GetAllCompaniesAsync()
        {
            var data = await _unitOfWork.Repository<CustomerCompany>().Query()
                .Where(x => !x.IsDeleted)
                .ToListAsync();
            return _mapper.Map<IEnumerable<CustomerCompanyDto>>(data);
        }

        public async Task<CustomerCompanyDto> GetCompanyByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<CustomerCompany>().GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return null;
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
            var entity = await _unitOfWork.Repository<CustomerCompany>().GetByIdAsync(dto.Id);
            if (entity == null || entity.IsDeleted) return null;

            _mapper.Map(dto, entity);
            _unitOfWork.Repository<CustomerCompany>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CustomerCompanyDto>(entity);
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var entity = await _unitOfWork.Repository<CustomerCompany>().GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            entity.IsDeleted = true;
            _unitOfWork.Repository<CustomerCompany>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // --- Customer Methods ---

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var data = await _unitOfWork.Repository<Customer>().Query()
                .Include(x => x.CustomerCompany)
                .Where(x => !x.IsDeleted)
                .ToListAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(data);
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
