using AutoMapper;
using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using S2O1.Domain.Enums;

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
            var product = new Product
            {
                ProductName = dto.ProductName,
                ProductCode = dto.ProductCode,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                UnitId = dto.UnitId,
                WarehouseId = dto.WarehouseId,
                CurrentStock = dto.InitialStock, // Initial stock from user input
                ImageUrl = dto.ImageUrl,
                IsPhysical = dto.IsPhysical,
                ShelfId = dto.ShelfId,
                IsActive = true,
                CreateDate = System.DateTime.Now
            };

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Generate unique product code if warehouse/shelf is provided
            if (product.WarehouseId.HasValue && product.ShelfId.HasValue)
            {
                await GenerateUniqueProductCode(product);
                _unitOfWork.Repository<Product>().Update(product);
                await _unitOfWork.SaveChangesAsync();
            }

            // If Initial Stock > 0, Create a Stock Movement Record
            if (dto.InitialStock > 0)
            {
                var userId = _currentUserService.UserId ?? 0;
                if (userId == 0) throw new System.Exception("User ID is missing. Cannot Create Stock Movement.");

                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    Quantity = dto.InitialStock,
                    MovementType = S2O1.Domain.Enums.MovementType.Entry,
                    MovementDate = System.DateTime.Now,
                    Description = "Initial Stock Entry",
                    WarehouseId = dto.WarehouseId.GetValueOrDefault(),
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    UserId = userId,
                    DocumentNo = "-"
                };
                await _unitOfWork.Repository<StockMovement>().AddAsync(movement);
                await _unitOfWork.SaveChangesAsync();
            }

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                SystemCode = product.SystemCode,
                WarehouseId = product.WarehouseId,
                CurrentStock = product.CurrentStock,
                ImageUrl = product.ImageUrl
            };
        }
        
        public async Task<System.Collections.Generic.IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Unit)
                .Include(p => p.PriceLists)
                .Include(p => p.Shelf)
                .Where(p => !p.IsDeleted)
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

        public async Task<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.BrandDto>> GetAllBrandsAsync()
        {
            var data = await _unitOfWork.Repository<Brand>().FindAsync(b => !b.IsDeleted);
            return _mapper.Map<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.BrandDto>>(data);
        }

        public async Task<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.CategoryDto>> GetAllCategoriesAsync()
        {
            var data = await _unitOfWork.Repository<Category>().FindAsync(c => !c.IsDeleted);
            return _mapper.Map<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.CategoryDto>>(data);
        }

        public async Task<System.Collections.Generic.IEnumerable<S2O1.Business.DTOs.Stock.UnitDto>> GetAllUnitsAsync()
        {
            var data = await _unitOfWork.Repository<ProductUnit>().FindAsync(u => !u.IsDeleted);
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

        public async Task<S2O1.Business.Services.Interfaces.ProductDto> UpdateAsync(S2O1.Business.Services.Interfaces.UpdateProductDto dto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.Id);
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
                if (userId == 0) throw new System.Exception("User ID is missing. Cannot Create Stock Movement.");

                product.CurrentStock += dto.AddedStock;
                
                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    Quantity = dto.AddedStock,
                    MovementType = S2O1.Domain.Enums.MovementType.Entry,
                    MovementDate = System.DateTime.Now,
                    Description = "Additional Stock Entry (Update)",
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
