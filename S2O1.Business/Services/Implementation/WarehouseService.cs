using AutoMapper;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Implementation
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        {
            var warehouses = await _unitOfWork.Repository<Warehouse>().FindAsync(w => !w.IsDeleted);
            var dtos = new List<WarehouseDto>();
            foreach (var w in warehouses)
            {
                dtos.Add(new WarehouseDto 
                { 
                    Id = w.Id, 
                    WarehouseName = w.WarehouseName, 
                    Location = w.Location, 
                    CompanyId = w.CompanyId 
                });
            }
            return dtos;
        }

        public async Task<WarehouseDto> GetByIdAsync(int id)
        {
            var w = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(id);
            if (w == null) return null;
            return new WarehouseDto 
            { 
                 Id = w.Id, 
                 WarehouseName = w.WarehouseName, 
                 Location = w.Location, 
                 CompanyId = w.CompanyId 
            };
        }

        public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto)
        {
            var warehouse = new Warehouse
            {
                WarehouseName = dto.WarehouseName,
                Location = dto.Location,
                CompanyId = dto.CompanyId
            };

            await _unitOfWork.Repository<Warehouse>().AddAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return new WarehouseDto 
            { 
                 Id = warehouse.Id, 
                 WarehouseName = warehouse.WarehouseName, 
                 Location = warehouse.Location, 
                 CompanyId = warehouse.CompanyId 
            };
        }

        public async Task<WarehouseDto> UpdateAsync(UpdateWarehouseDto dto)
        {
            var warehouse = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(dto.Id);
            if (warehouse == null) return null;

            warehouse.WarehouseName = dto.WarehouseName;
            warehouse.Location = dto.Location;
            warehouse.CompanyId = dto.CompanyId;

            _unitOfWork.Repository<Warehouse>().Update(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return new WarehouseDto 
            { 
                 Id = warehouse.Id, 
                 WarehouseName = warehouse.WarehouseName, 
                 Location = warehouse.Location, 
                 CompanyId = warehouse.CompanyId 
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warehouse = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(id);
            if (warehouse == null) return false;

            warehouse.IsDeleted = true;
            _unitOfWork.Repository<Warehouse>().Update(warehouse);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
