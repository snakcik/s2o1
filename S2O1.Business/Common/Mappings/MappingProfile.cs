using AutoMapper;
using S2O1.Domain.Entities;
using S2O1.Business.DTOs.Stock;
using S2O1.Business.DTOs.Auth;
using S2O1.Business.DTOs.Invoice;
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
            CreateMap<Offer, OfferDto>().ReverseMap();
            CreateMap<OfferItem, OfferItemDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Brand, CreateBrandDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<ProductUnit, UnitDto>().ForMember(d => d.UnitName, o => o.MapFrom(s => s.UnitName)).ReverseMap();
            CreateMap<ProductUnit, CreateUnitDto>().ReverseMap();

            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, CreateProductDto>().ReverseMap();
            CreateMap<Product, UpdateProductDto>().ReverseMap();
        }
    }
}
