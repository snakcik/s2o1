using AutoMapper;
using System.Linq;
using S2O1.Domain.Entities;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.DTOs.Invoice;
using S2O1.Business.DTOs.Business;
using S2O1.Business.Services.Interfaces;

namespace S2O1.Business.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StockMovement, StockMovementDto>().ReverseMap();
            CreateMap<User, UserDto>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.UserMail))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.UserFirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.UserLastName))
                .ReverseMap();
            // Add more mappings
            CreateMap<Invoice, InvoiceDto>().ReverseMap();
            CreateMap<InvoiceItem, InvoiceItemDto>().ReverseMap();
            CreateMap<Offer, OfferDto>()
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.CustomerCompany.CustomerCompanyName))
                .ReverseMap();
            CreateMap<OfferItem, OfferItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product.ProductCode))
                .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Product.ImageUrl))
                .ReverseMap();
            CreateMap<Offer, CreateOfferDto>().ReverseMap();
            CreateMap<OfferItem, CreateOfferItemDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Brand, CreateBrandDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<ProductUnit, UnitDto>().ForMember(d => d.UnitName, o => o.MapFrom(s => s.UnitName)).ReverseMap();
            CreateMap<ProductUnit, CreateUnitDto>().ReverseMap();

            CreateMap<Product, ProductDto>()
                .ForMember(d => d.UnitName, o => o.MapFrom(s => s.Unit.UnitName))
                .ForMember(d => d.CurrentPrice, o => o.MapFrom(s => s.PriceLists.FirstOrDefault(p => p.IsActivePrice).SalePrice))
                .ForMember(d => d.Currency, o => o.MapFrom(s => s.PriceLists.FirstOrDefault(p => p.IsActivePrice).Currency))
                .ReverseMap();
            CreateMap<Product, CreateProductDto>().ReverseMap();
            CreateMap<Product, UpdateProductDto>().ReverseMap();

            CreateMap<Supplier, S2O1.Business.DTOs.Business.SupplierDto>().ReverseMap();
            CreateMap<Supplier, S2O1.Business.DTOs.Business.CreateSupplierDto>().ReverseMap();
            CreateMap<Supplier, S2O1.Business.DTOs.Business.UpdateSupplierDto>().ReverseMap();

            CreateMap<PriceList, PriceListDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product.ProductCode))
                .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.SupplierCompanyName))
                .ReverseMap();
            CreateMap<PriceList, CreatePriceListDto>().ReverseMap();
            CreateMap<PriceList, UpdatePriceListDto>().ReverseMap();

            CreateMap<CustomerCompany, CustomerCompanyDto>().ReverseMap();
            CreateMap<CustomerCompany, CreateCustomerCompanyDto>().ReverseMap();
            CreateMap<CustomerCompany, UpdateCustomerCompanyDto>().ReverseMap();

            CreateMap<Customer, CustomerDto>()
                .ForMember(d => d.CustomerCompanyName, o => o.MapFrom(s => s.CustomerCompany.CustomerCompanyName))
                .ReverseMap();
            CreateMap<Customer, CreateCustomerDto>().ReverseMap();
            CreateMap<Customer, UpdateCustomerDto>().ReverseMap();
        }
    }
}
