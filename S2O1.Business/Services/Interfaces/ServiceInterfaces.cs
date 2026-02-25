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
        Task<S2O1.Business.DTOs.Common.PagedResultDto<UserDto>> GetAllUsersAsync(int? currentUserId = null, string? status = null, string? requiredModule = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<IEnumerable<ModuleDto>> GetAllModulesAsync();
        Task<IEnumerable<UserPermissionDto>> GetUserPermissionsAsync(int userId);
        Task<bool> SaveUserPermissionsAsync(int userId, IEnumerable<UserPermissionDto> permissions);
        Task<bool> DeleteUserAsync(int userId);
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<bool> ForgotPasswordAsync(string email, string baseUrl);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        
        // Title Management
        Task<IEnumerable<TitleDto>> GetAllTitlesAsync();
        Task<TitleDto> GetTitleByIdAsync(int id);
        Task<IEnumerable<TitleDto>> GetTitlesByCompanyAsync(int companyId);
        Task<TitleDto> CreateTitleAsync(CreateTitleDto dto);
        Task<TitleDto> UpdateTitleAsync(int id, CreateTitleDto dto);
        Task<bool> DeleteTitleAsync(int id);
        
        // Title Permissions
        Task<IEnumerable<TitlePermissionDto>> GetTitlePermissionsAsync(int titleId);
        Task<bool> SaveTitlePermissionsAsync(int titleId, IEnumerable<TitlePermissionDto> permissions);
    }

    public interface IMailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendOfferEmailAsync(string to, string subject, string htmlContent, string fileName);
    }

    public interface ICompanyService
    {
        Task<S2O1.Business.DTOs.Common.PagedResultDto<CompanyDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<CompanyDto> GetByIdAsync(int id);
        Task<CompanyDto> CreateAsync(CreateCompanyDto dto);
        Task<CompanyDto> UpdateAsync(int id, CreateCompanyDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IStockService
    {
        Task CreateMovementAsync(StockMovementDto movementDto);
        Task<decimal> GetProductStockAsync(int productId, int warehouseId);
        Task<System.Collections.Generic.IEnumerable<WarehouseStockReportDto>> GetWarehouseStockReportAsync(int? warehouseId);
        Task<System.Collections.Generic.IEnumerable<WaybillDto>> GetWaybillsBySupplierAsync(int supplierId);
        Task<System.Collections.Generic.IEnumerable<WaybillDto>> SearchWaybillsAsync(string? waybillNo, System.DateTime? startDate, System.DateTime? endDate, int? supplierId, string? type);
        Task<System.Collections.Generic.IEnumerable<WaybillItemDto>> GetWaybillItemsAsync(string waybillNo);
    }

    public interface IWarehouseService
    {
        Task<S2O1.Business.DTOs.Common.PagedResultDto<WarehouseDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<WarehouseDto> GetByIdAsync(int id);
        Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto);
        Task<WarehouseDto> UpdateAsync(UpdateWarehouseDto dto);
        Task<bool> DeleteAsync(int id);

        // Shelves
        Task<IEnumerable<WarehouseShelfDto>> GetAllShelvesAsync(string? status = null);
        Task<IEnumerable<WarehouseShelfDto>> GetShelvesAsync(int warehouseId);
        Task<WarehouseShelfDto> CreateShelfAsync(CreateWarehouseShelfDto dto);
        Task<bool> DeleteShelfAsync(int id);
    }

    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(int id);
        Task<S2O1.Business.DTOs.Common.PagedResultDto<ProductDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<System.Collections.Generic.IEnumerable<BrandDto>> GetAllBrandsAsync(string? status = null, string? searchTerm = null);
        Task<System.Collections.Generic.IEnumerable<CategoryDto>> GetAllCategoriesAsync(string? status = null, string? searchTerm = null);
        Task<System.Collections.Generic.IEnumerable<UnitDto>> GetAllUnitsAsync(string? status = null, string? searchTerm = null);
        
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
        Task<S2O1.Business.DTOs.Common.PagedResultDto<S2O1.Business.DTOs.Stock.OfferDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<S2O1.Business.DTOs.Stock.OfferDto> GetByIdAsync(int id);
        Task<S2O1.Business.DTOs.Stock.OfferDto> CreateAsync(S2O1.Business.DTOs.Stock.CreateOfferDto dto);
        Task<S2O1.Business.DTOs.Stock.OfferDto> UpdateAsync(int id, S2O1.Business.DTOs.Stock.CreateOfferDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IInvoiceService
    {
        Task<InvoiceDto> GetByIdAsync(int id);
        Task<InvoiceDto> GetByNumberAsync(string invoiceNumber);
        Task<S2O1.Business.DTOs.Common.PagedResultDto<InvoiceDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto);
        Task<bool> ApproveInvoiceAsync(int invoiceId, int approverUserId);
        Task<bool> RejectInvoiceAsync(int invoiceId);
        
        // Warehouse Workflow
        Task<IEnumerable<InvoiceDto>> GetPendingDeliveriesAsync();
        Task<bool> AssignToDelivererAsync(int invoiceId, int userId);
        Task<bool> CompleteDeliveryAsync(WarehouseDeliveryDto dto);
        Task<bool> MarkAsIncompleteAsync(int invoiceId);
        Task<bool> TransferJobAsync(int invoiceId, int toUserId);
        Task<bool> UnassignJobAsync(int invoiceId);
    }

    public interface ISupplierService
    {
        Task<S2O1.Business.DTOs.Common.PagedResultDto<S2O1.Business.DTOs.Business.SupplierDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<S2O1.Business.DTOs.Business.SupplierDto> GetByIdAsync(int id);
        Task<S2O1.Business.DTOs.Business.SupplierDto> CreateAsync(S2O1.Business.DTOs.Business.CreateSupplierDto dto);
        Task<S2O1.Business.DTOs.Business.SupplierDto> UpdateAsync(S2O1.Business.DTOs.Business.UpdateSupplierDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IPriceListService
    {
        Task<S2O1.Business.DTOs.Common.PagedResultDto<PriceListDto>> GetAllAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<PriceListDto> GetByIdAsync(int id);
        Task<PriceListDto> CreateAsync(CreatePriceListDto dto);
        Task<PriceListDto> UpdateAsync(UpdatePriceListDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface ICustomerService
    {
        // Customer Company
        Task<S2O1.Business.DTOs.Common.PagedResultDto<CustomerCompanyDto>> GetAllCompaniesAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<CustomerCompanyDto> GetCompanyByIdAsync(int id);
        Task<CustomerCompanyDto> CreateCompanyAsync(CreateCustomerCompanyDto dto);
        Task<CustomerCompanyDto> UpdateCompanyAsync(UpdateCustomerCompanyDto dto);
        Task<bool> DeleteCompanyAsync(int id);

        // Customer (Contact)
        Task<S2O1.Business.DTOs.Common.PagedResultDto<CustomerDto>> GetAllCustomersAsync(string? status = null, string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerDto dto);
        Task<bool> DeleteCustomerAsync(int id);
    }

    public interface IDispatchNoteService
    {
        Task<S2O1.Business.DTOs.Common.PagedResultDto<S2O1.Business.DTOs.Logistic.DispatchNoteDto>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<S2O1.Business.DTOs.Logistic.DispatchNoteDto> GetByIdAsync(int id);
        Task<S2O1.Business.DTOs.Logistic.DispatchNoteDto> CreateAsync(S2O1.Business.DTOs.Logistic.CreateDispatchNoteDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        // More methods for Phase 3 (Assignment, etc.)
    }
}
