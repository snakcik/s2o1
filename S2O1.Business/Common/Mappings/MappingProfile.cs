using AutoMapper;
using System.Linq;
using S2O1.Domain.Entities;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.DTOs.Invoice;
using S2O1.Business.DTOs.Business;
using S2O1.Business.Services.Interfaces;
using S2O1.Business.DTOs.Logistic;

namespace S2O1.Business.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StockMovement, StockMovementDto>().ReverseMap();
            CreateMap<UserPermission, UserPermissionDto>()
                .ForMember(d => d.ModuleName, o => o.MapFrom(s => s.Module.ModuleName))
                .ReverseMap();

            CreateMap<User, UserDto>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.UserMail))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.UserFirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.UserLastName))
                .ForMember(d => d.RegNo, o => o.MapFrom(s => s.UserRegNo))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.RoleName))
                .ForMember(d => d.TitleName, o => o.MapFrom(s => s.Title.TitleName))
                .ForMember(d => d.Permissions, o => o.MapFrom(s => s.Permissions))
                .ReverseMap();
            // Add more mappings
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(d => d.AssignedDelivererUserName, o => o.MapFrom(s => s.AssignedDelivererUser != null ? s.AssignedDelivererUser.UserFirstName + " " + s.AssignedDelivererUser.UserLastName : null))
                .ForMember(d => d.BuyerCompanyName, o => o.MapFrom(s => s.BuyerCompany != null ? s.BuyerCompany.CustomerCompanyName : (s.Offer != null && s.Offer.Customer != null && s.Offer.Customer.CustomerCompany != null ? s.Offer.Customer.CustomerCompany.CustomerCompanyName : "")))
                .ForMember(d => d.BuyerCompanyAddress, o => o.MapFrom(s => s.BuyerCompany != null ? s.BuyerCompany.CustomerCompanyAddress : (s.Offer != null && s.Offer.Customer != null && s.Offer.Customer.CustomerCompany != null ? s.Offer.Customer.CustomerCompany.CustomerCompanyAddress : "")))
                .ForMember(d => d.BuyerCompanyTaxInfo, o => o.MapFrom(s => s.BuyerCompany != null ? s.BuyerCompany.CustomerCompanyMail : (s.Offer != null && s.Offer.Customer != null && s.Offer.Customer.CustomerCompany != null ? s.Offer.Customer.CustomerCompany.CustomerCompanyMail : "")))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
                
            CreateMap<InvoiceItem, InvoiceItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.ProductName : ""))
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.ProductCode : ""))
                .ForMember(d => d.WarehouseName, o => o.MapFrom(s => (s.Product != null && s.Product.Warehouse != null) ? s.Product.Warehouse.WarehouseName : ""))
                .ForMember(d => d.ShelfName, o => o.MapFrom(s => (s.Product != null && s.Product.Shelf != null) ? s.Product.Shelf.Name : ""))
                .ReverseMap();
            CreateMap<Offer, OfferDto>()
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.CustomerCompany.CustomerCompanyName))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<OfferItem, OfferItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.ProductName : ""))
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.ProductCode : ""))
                .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Product != null ? s.Product.ImageUrl : ""))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => (s.Product != null && s.Product.Category != null) ? s.Product.Category.CategoryName : "Diğer"))
                .ReverseMap();
            CreateMap<Offer, CreateOfferDto>().ReverseMap();
            CreateMap<OfferItem, CreateOfferItemDto>().ReverseMap();
            CreateMap<Brand, BrandDto>()
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<Brand, CreateBrandDto>().ReverseMap();
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<ProductUnit, UnitDto>()
                .ForMember(d => d.UnitName, o => o.MapFrom(s => s.UnitName))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<ProductUnit, CreateUnitDto>().ReverseMap();

            CreateMap<Product, ProductDto>()
                .ForMember(d => d.UnitName, o => o.MapFrom(s => s.Unit != null ? s.Unit.UnitName : null))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.CategoryName : null))
                .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.BrandName : null))
                .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.WarehouseName : null))
                .ForMember(d => d.CurrentPrice, o => o.MapFrom(s => s.PriceLists.Where(p => p.IsActivePrice).Select(p => p.SalePrice).FirstOrDefault()))
                .ForMember(d => d.Currency, o => o.MapFrom(s => s.PriceLists.Where(p => p.IsActivePrice).Select(p => p.Currency).FirstOrDefault()))
                .ForMember(d => d.ShelfName, o => o.MapFrom(s => s.Shelf != null ? s.Shelf.Name : null))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<Product, CreateProductDto>().ReverseMap();
            CreateMap<Product, UpdateProductDto>().ReverseMap();

            CreateMap<Supplier, S2O1.Business.DTOs.Business.SupplierDto>()
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<Supplier, S2O1.Business.DTOs.Business.CreateSupplierDto>().ReverseMap();
            CreateMap<Supplier, S2O1.Business.DTOs.Business.UpdateSupplierDto>().ReverseMap();

            CreateMap<PriceList, PriceListDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product.ProductCode))
                .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.SupplierCompanyName))
                .ReverseMap();
            CreateMap<PriceList, CreatePriceListDto>().ReverseMap();
            CreateMap<PriceList, UpdatePriceListDto>().ReverseMap();

            CreateMap<CustomerCompany, CustomerCompanyDto>()
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<CustomerCompany, CreateCustomerCompanyDto>().ReverseMap();
            CreateMap<CustomerCompany, UpdateCustomerCompanyDto>().ReverseMap();

            CreateMap<Customer, CustomerDto>()
                .ForMember(d => d.CustomerCompanyName, o => o.MapFrom(s => s.CustomerCompany.CustomerCompanyName))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<Customer, CreateCustomerDto>().ReverseMap();
            CreateMap<Customer, UpdateCustomerDto>().ReverseMap();

            CreateMap<Title, TitleDto>().ReverseMap();
            CreateMap<Title, CreateTitleDto>().ReverseMap();
            
            CreateMap<TitlePermission, TitlePermissionDto>()
                .ForMember(d => d.ModuleName, o => o.MapFrom(s => s.Module.ModuleName))
                .ReverseMap();

            // Logistic
            CreateMap<WarehouseShelf, WarehouseShelfDto>()
                .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.WarehouseName : ""))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
                .ReverseMap();
            CreateMap<WarehouseShelf, CreateWarehouseShelfDto>().ReverseMap();

            CreateMap<DispatchNote, DispatchNoteDto>()
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company.CompanyName))
                // CustomerName - Simple combination
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.CustomerContactPersonName + " " + s.Customer.CustomerContactPersonLastName : ""))
                .ReverseMap();

            CreateMap<DispatchNote, CreateDispatchNoteDto>().ReverseMap();

            CreateMap<DispatchNoteItem, DispatchNoteItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product.ProductCode))
                .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Product != null && s.Product.Brand != null ? s.Product.Brand.BrandName : ""))
                .ReverseMap();

            CreateMap<DispatchNoteItem, CreateDispatchNoteItemDto>().ReverseMap();
        }
    }
}
