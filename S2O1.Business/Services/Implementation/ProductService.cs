using AutoMapper;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using S2O1.Domain.Enums;
using S2O1.Business.DTOs.Stock;

namespace S2O1.Business.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var p = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Unit)
                .Include(p => p.PriceLists)
                .Include(p => p.Shelf)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return null;
            
            var reserved = await _unitOfWork.Repository<OfferItem>().Query()
                .Include(o => o.Offer)
                .Where(o => o.ProductId == id && (o.Offer.Status == OfferStatus.Pending || o.Offer.Status == OfferStatus.Approved) && !o.Offer.IsDeleted)
                .SumAsync(o => o.Quantity);

            var dto = _mapper.Map<ProductDto>(p);
            dto.ReservedStock = reserved;
            return dto;
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            // 1. Validate uniqueness BEFORE touching DB
            var duplicate = await _unitOfWork.Repository<Product>().Query()
                .IgnoreQueryFilters()
                .AnyAsync(p => p.ProductCode == dto.ProductCode && p.WarehouseId == dto.WarehouseId);
            if (duplicate)
            {
                throw new System.InvalidOperationException(
                    $"Bu depoda '{dto.ProductCode}' ürün koduyla zaten bir ürün mevcut.");
            }

            var product = new Product
            {
                ProductName = dto.ProductName,
                ProductCode = dto.ProductCode,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                UnitId = dto.UnitId,
                WarehouseId = dto.WarehouseId,
                CurrentStock = dto.InitialStock,
                ImageUrl = dto.ImageUrl,
                IsPhysical = dto.IsPhysical,
                ShelfId = dto.ShelfId,
                IsActive = true,
                CreateDate = System.DateTime.Now
            };

            // 2. Generate unique system code BEFORE first save (so it is included in the same commit)
            if (product.WarehouseId.HasValue && product.ShelfId.HasValue)
            {
                await GenerateUniqueProductCode(product);
            }

            await _unitOfWork.Repository<Product>().AddAsync(product);

            // 3. Queue stock movement in same unit-of-work (no extra SaveChanges needed)
            StockMovement movement = null;
            if (dto.InitialStock > 0)
            {
                var userId = _currentUserService.UserId ?? 0;
                if (userId == 0)
                    throw new System.Exception("Kullanıcı kimliği bulunamadı. Stok hareketi oluşturulamadı.");

                movement = new StockMovement
                {
                    Product = product, // EF will link via FK after insert
                    Quantity = dto.InitialStock,
                    MovementType = S2O1.Domain.Enums.MovementType.Entry,
                    MovementDate = System.DateTime.Now,
                    Description = "İlk Stok Girişi",
                    WarehouseId = dto.WarehouseId ?? 0,
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    UserId = userId,
                    DocumentNo = "-"
                };
                await _unitOfWork.Repository<StockMovement>().AddAsync(movement);
            }

            // 4. Single atomic commit — product + movement saved together
            await _unitOfWork.SaveChangesAsync();

            // 5. Reload with navigation properties for the DTO
            product = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Unit)
                .Include(p => p.PriceLists)
                .Include(p => p.Shelf)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            return _mapper.Map<ProductDto>(product);
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

        public async Task<System.Collections.Generic.IEnumerable<ProductDto>> GetAllAsync(string? status = null, string? searchTerm = null)
        {
            var canSeeDeleted = await CanSeeDeletedAsync();
            IQueryable<Product> query = _unitOfWork.Repository<Product>().Query();

            if (canSeeDeleted)
            {
                query = query.IgnoreQueryFilters();
                if (status == "passive") query = query.Where(p => p.IsDeleted);
                else if (status == "all") query = query.Where(p => true);
                else query = query.Where(p => !p.IsDeleted);
            }
            else
            {
                // If can't see deleted, return empty if "passive" was requested
                if (status == "passive") return new List<ProductDto>();
                query = query.Where(p => !p.IsDeleted);
            }
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(p => 
                    p.ProductName.ToLower().Contains(search) || 
                    p.ProductCode.ToLower().Contains(search) || 
                    (p.SystemCode != null && p.SystemCode.ToLower().Contains(search)) ||
                    (p.Category != null && p.Category.CategoryName.ToLower().Contains(search)) || 
                    (p.Brand != null && p.Brand.BrandName.ToLower().Contains(search)) ||
                    (p.Warehouse != null && p.Warehouse.WarehouseName.ToLower().Contains(search))
                );
            }

            var products = await query
                .Include(p => p.Unit)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Warehouse)
                .Include(p => p.PriceLists)
                .Include(p => p.Shelf)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var reserved = await _unitOfWork.Repository<OfferItem>().Query()
                .Include(o => o.Offer)
                .Where(o => (o.Offer.Status == OfferStatus.Pending || o.Offer.Status == OfferStatus.Approved) && !o.Offer.IsDeleted)
                .GroupBy(o => o.ProductId)
                .Select(g => new { ProductId = g.Key, Reserved = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var result = _mapper.Map<System.Collections.Generic.IEnumerable<ProductDto>>(products);
            foreach (var p in result)
            {
                p.ReservedStock = reserved.FirstOrDefault(r => r.ProductId == p.Id)?.Reserved ?? 0;
            }
            return result;
        }

        public async Task<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.BrandDto>> GetAllBrandsAsync(string? status = null, string? searchTerm = null)
        {
            var query = _unitOfWork.Repository<Brand>().Query();
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
                query = query.Where(x => x.BrandName.ToLower().Contains(search));
            }

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.BrandDto>>(data);
        }

        public async Task<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.CategoryDto>> GetAllCategoriesAsync(string? status = null, string? searchTerm = null)
        {
            var query = _unitOfWork.Repository<Category>().Query();
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
                query = query.Where(x => x.CategoryName.ToLower().Contains(search));
            }

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.CategoryDto>>(data);
        }

        public async Task<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.UnitDto>> GetAllUnitsAsync(string? status = null, string? searchTerm = null)
        {
            var query = _unitOfWork.Repository<ProductUnit>().Query();
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
                query = query.Where(x => x.UnitName.ToLower().Contains(search) || x.UnitShortName.ToLower().Contains(search));
            }

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.UnitDto>>(data);
        }

        public async Task<S2O1.Business.DTOs.Stock.BrandDto> CreateBrandAsync(S2O1.Business.DTOs.Stock.CreateBrandDto dto)
        {
            var entity = _mapper.Map<Brand>(dto);
            if(string.IsNullOrEmpty(entity.BrandDescription)) entity.BrandDescription = "-"; // Default
            if (entity.BrandLogo == null) entity.BrandLogo = new byte[0]; // Satisfy NotNull constraint
            await _unitOfWork.Repository<Brand>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<S2O1.Business.DTOs.Stock.BrandDto>(entity);
        }

        public async Task<S2O1.Business.DTOs.Stock.CategoryDto> CreateCategoryAsync(S2O1.Business.DTOs.Stock.CreateCategoryDto dto)
        {
            var entity = _mapper.Map<Category>(dto);
            if (string.IsNullOrEmpty(entity.CategoryDescription)) entity.CategoryDescription = "-"; // Default
            await _unitOfWork.Repository<Category>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<S2O1.Business.DTOs.Stock.CategoryDto>(entity);
        }

        public async Task<S2O1.Business.DTOs.Stock.UnitDto> CreateUnitAsync(S2O1.Business.DTOs.Stock.CreateUnitDto dto)
        {
            var entity = _mapper.Map<ProductUnit>(dto);
            await _unitOfWork.Repository<ProductUnit>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<S2O1.Business.DTOs.Stock.UnitDto>(entity);
        }

        public async Task<S2O1.Business.DTOs.Stock.BrandDto> UpdateBrandAsync(S2O1.Business.DTOs.Stock.UpdateBrandDto dto)
        {
            var entity = await _unitOfWork.Repository<Brand>().GetByIdAsync(dto.Id);
            if (entity == null) return null;

            entity.BrandName = dto.BrandName;
            if (!string.IsNullOrEmpty(dto.BrandDescription)) entity.BrandDescription = dto.BrandDescription;
            
            _unitOfWork.Repository<Brand>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<S2O1.Business.DTOs.Stock.BrandDto>(entity);
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Brand>().GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            _unitOfWork.Repository<Brand>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<S2O1.Business.DTOs.Stock.CategoryDto> UpdateCategoryAsync(S2O1.Business.DTOs.Stock.UpdateCategoryDto dto)
        {
            var entity = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.Id);
            if (entity == null) return null;

            entity.CategoryName = dto.CategoryName;
            if (!string.IsNullOrEmpty(dto.CategoryDescription)) entity.CategoryDescription = dto.CategoryDescription;
            entity.ParentCategoryId = dto.ParentCategoryId;

            _unitOfWork.Repository<Category>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<S2O1.Business.DTOs.Stock.CategoryDto>(entity);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            _unitOfWork.Repository<Category>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<S2O1.Business.DTOs.Stock.UnitDto> UpdateUnitAsync(S2O1.Business.DTOs.Stock.UpdateUnitDto dto)
        {
            var entity = await _unitOfWork.Repository<ProductUnit>().GetByIdAsync(dto.Id);
            if (entity == null) return null;

            entity.UnitName = dto.UnitName;
            entity.UnitShortName = dto.UnitShortName;
            entity.IsDecimal = dto.IsDecimal;

            _unitOfWork.Repository<ProductUnit>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<S2O1.Business.DTOs.Stock.UnitDto>(entity);
        }

        public async Task<bool> DeleteUnitAsync(int id)
        {
            var entity = await _unitOfWork.Repository<ProductUnit>().GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            _unitOfWork.Repository<ProductUnit>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductDto dto)
        {
            var product = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Unit)
                .Include(p => p.PriceLists)
                .Include(p => p.Shelf)
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (product == null) return null;

            product.ProductName = dto.ProductName;
            product.ProductCode = dto.ProductCode;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.UnitId = dto.UnitId;
            product.WarehouseId = dto.WarehouseId;
            product.ImageUrl = dto.ImageUrl;
            product.IsPhysical = dto.IsPhysical;
            product.ShelfId = dto.ShelfId;

            if (dto.AddedStock > 0)
            {
                var userId = _currentUserService.UserId ?? 0;
                if (userId == 0) throw new System.Exception("Kullanıcı kimliği bulunamadı. Stok hareketi oluşturulamadı.");

                product.CurrentStock += dto.AddedStock;
                
                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    Quantity = dto.AddedStock,
                    MovementType = S2O1.Domain.Enums.MovementType.Entry,
                    MovementDate = System.DateTime.Now,
                    Description = "Ek Stok Girişi (Güncelleme)",
                    WarehouseId = dto.WarehouseId.GetValueOrDefault(), 
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    UserId = userId,
                    DocumentNo = "-"
                };
                await _unitOfWork.Repository<StockMovement>().AddAsync(movement);
            }

            // Update unique code if warehouse/shelf changed
            await GenerateUniqueProductCode(product);

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return false;

            product.IsDeleted = true;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task GenerateUniqueProductCode(Product product)
        {
            if (product.WarehouseId.HasValue && product.ShelfId.HasValue)
            {
                var warehouse = await _unitOfWork.Repository<Warehouse>().Query()
                    .Include(w => w.Company)
                    .FirstOrDefaultAsync(w => w.Id == product.WarehouseId.Value);

                if (warehouse != null)
                {
                    // Format: C[CompId]-W[WhId]-S[ShelfId]-P[ProdId]
                    product.SystemCode = $"C{warehouse.CompanyId}-W{product.WarehouseId}-S{product.ShelfId}-P{product.Id}";
                }
            }
            else
            {
                // Fallback for non-warehouse products or products without shelf
                product.SystemCode = $"P{product.Id}-{System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            }
        }
    }
}
