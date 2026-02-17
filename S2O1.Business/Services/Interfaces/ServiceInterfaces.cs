using S2O1.Business.DTOs.Auth;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.DTOs.Invoice;
using S2O1.Business.DTOs.Business;
using System.Threading.Tasks;

namespace S2O1.Business.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> LoginAsync(LoginDto loginDto);
        Task<string> GenerateTokenAsync(UserDto user);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<bool> AssignRoleAsync(int userId, int roleId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync(int? currentUserId = null);
        Task<IEnumerable<ModuleDto>> GetAllModulesAsync();
        Task<IEnumerable<UserPermissionDto>> GetUserPermissionsAsync(int userId);
        Task<bool> SaveUserPermissionsAsync(int userId, IEnumerable<UserPermissionDto> permissions);
        Task<bool> DeleteUserAsync(int userId);
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto);
    }

    public interface ICompanyService
    {
        Task<IEnumerable<CompanyDto>> GetAllAsync();
        Task<CompanyDto> CreateAsync(CreateCompanyDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IStockService
    {
        Task CreateMovementAsync(StockMovementDto movementDto);
        Task<decimal> GetProductStockAsync(int productId, int warehouseId);
        Task<System.Collections.Generic.IEnumerable<WarehouseStockReportDto>> GetWarehouseStockReportAsync(int? warehouseId);
    }

    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseDto>> GetAllAsync();
        Task<WarehouseDto> GetByIdAsync(int id);
        Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto);
        Task<WarehouseDto> UpdateAsync(UpdateWarehouseDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(int id);
        Task<System.Collections.Generic.IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<System.Collections.Generic.IEnumerable<BrandDto>> GetAllBrandsAsync();
        Task<System.Collections.Generic.IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<System.Collections.Generic.IEnumerable<UnitDto>> GetAllUnitsAsync();
        
        Task<BrandDto> CreateBrandAsync(CreateBrandDto dto);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<UnitDto> CreateUnitAsync(CreateUnitDto dto);
        
        Task<BrandDto> UpdateBrandAsync(UpdateBrandDto dto);
        Task<bool> DeleteBrandAsync(int id);
        
        Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        
        Task<UnitDto> UpdateUnitAsync(UpdateUnitDto dto);
        Task<bool> DeleteUnitAsync(int id);
        Task<ProductDto> UpdateAsync(UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IOfferService
    {
        Task ApproveOfferAsync(int offerId, int approverUserId);
        Task<int> CreateInvoiceFromOfferAsync(int offerId, int userId);
        Task<IEnumerable<S2O1.Business.DTOs.Stock.OfferDto>> GetAllAsync();
        Task<S2O1.Business.DTOs.Stock.OfferDto> GetByIdAsync(int id);
        Task<S2O1.Business.DTOs.Stock.OfferDto> CreateAsync(S2O1.Business.DTOs.Stock.CreateOfferDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IInvoiceService
    {
        Task<InvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetAllAsync(); // Added
        Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto);
        Task<bool> ApproveInvoiceAsync(int invoiceId, int approverUserId);
    }

    public interface ISupplierService
    {
        Task<IEnumerable<S2O1.Business.DTOs.Business.SupplierDto>> GetAllAsync();
        Task<S2O1.Business.DTOs.Business.SupplierDto> GetByIdAsync(int id);
        Task<S2O1.Business.DTOs.Business.SupplierDto> CreateAsync(S2O1.Business.DTOs.Business.CreateSupplierDto dto);
        Task<S2O1.Business.DTOs.Business.SupplierDto> UpdateAsync(S2O1.Business.DTOs.Business.UpdateSupplierDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IPriceListService
    {
        Task<IEnumerable<PriceListDto>> GetAllAsync();
        Task<PriceListDto> GetByIdAsync(int id);
        Task<PriceListDto> CreateAsync(CreatePriceListDto dto);
        Task<PriceListDto> UpdateAsync(UpdatePriceListDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface ICustomerService
    {
        // Customer Company
        Task<IEnumerable<CustomerCompanyDto>> GetAllCompaniesAsync();
        Task<CustomerCompanyDto> GetCompanyByIdAsync(int id);
        Task<CustomerCompanyDto> CreateCompanyAsync(CreateCustomerCompanyDto dto);
        Task<CustomerCompanyDto> UpdateCompanyAsync(UpdateCustomerCompanyDto dto);
        Task<bool> DeleteCompanyAsync(int id);

        // Customer (Contact)
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerDto dto);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
