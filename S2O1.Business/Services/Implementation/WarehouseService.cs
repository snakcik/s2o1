using AutoMapper;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace S2O1.Business.Services.Implementation
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
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

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync(string? status = null, string? searchTerm = null)
        {
            var query = _unitOfWork.Repository<Warehouse>().Query();
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
                query = query.Where(x => x.WarehouseName.ToLower().Contains(search));
            }

            var warehouses = await query.Include(x => x.Company).OrderByDescending(x => x.Id).ToListAsync();
            var dtos = new List<WarehouseDto>();
            foreach (var w in warehouses)
            {
                dtos.Add(new WarehouseDto 
                { 
                    Id = w.Id, 
                    WarehouseName = w.WarehouseName, 
                    Location = w.Location, 
                    CompanyId = w.CompanyId,
                    CompanyName = w.Company?.CompanyName,
                    IsDeleted = w.IsDeleted
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

        public async Task<IEnumerable<WarehouseShelfDto>> GetAllShelvesAsync(string? status = null)
        {
            IQueryable<WarehouseShelf> query = _unitOfWork.Repository<WarehouseShelf>().Query();

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

            var shelves = await query.Include(s => s.Warehouse).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<IEnumerable<WarehouseShelfDto>>(shelves);
        }

        public async Task<IEnumerable<WarehouseShelfDto>> GetShelvesAsync(int warehouseId)
        {
            var shelves = await _unitOfWork.Repository<WarehouseShelf>().Query()
                .Include(s => s.Warehouse)
                .Where(s => s.WarehouseId == warehouseId && !s.IsDeleted)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
            return _mapper.Map<IEnumerable<WarehouseShelfDto>>(shelves);
        }

        public async Task<WarehouseShelfDto> CreateShelfAsync(CreateWarehouseShelfDto dto)
        {
            // Manual mapping or AutoMapper? Using AutoMapper since I configured it.
            var shelf = _mapper.Map<WarehouseShelf>(dto);
            await _unitOfWork.Repository<WarehouseShelf>().AddAsync(shelf);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<WarehouseShelfDto>(shelf);
        }

        public async Task<bool> DeleteShelfAsync(int id)
        {
             var shelf = await _unitOfWork.Repository<WarehouseShelf>().GetByIdAsync(id);
             if (shelf == null) return false;
             
             shelf.IsDeleted = true;
             _unitOfWork.Repository<WarehouseShelf>().Update(shelf);
             await _unitOfWork.SaveChangesAsync();
             return true;
        }
    }
}
