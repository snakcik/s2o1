
using S2O1.Core.Security; // Added
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using S2O1.Business.Services.Interfaces;
using S2O1.Business.Services.Implementation;

namespace S2O1.Business
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>(); // Added
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<ILicenseService, LicenseService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IPriceListService, PriceListService>();
            services.AddScoped<ICustomerService, CustomerService>();
            
            // ICurrentUserService needs to be implemented by Consumer (API/CLI).

            return services;
        }
    }
}
