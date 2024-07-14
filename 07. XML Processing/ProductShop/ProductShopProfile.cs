using AutoMapper;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            // Imports

            CreateMap<ImportUserDto, User>();
            CreateMap<ImportProductDto, Product>();
            CreateMap<ImportCategoryDto, Category>();
            CreateMap<ImportCategoryProductDto, CategoryProduct>();

            // Exports

            CreateMap<Product, ExportProductDto>()
                .ForMember(d => d.Buyer,
                    opt => opt.MapFrom(s =>
                        $"{s.Buyer.FirstName} {s.Buyer.LastName}"));



            //CreateMap<Product, ExportProductDto>()
            //    .ForMember(d => d.Buyer,
            //        opt => opt.MapFrom(s =>
            //            s.Buyer != null ? $"{s.Buyer.FirstName} {s.Buyer.LastName}" : null))
            //    .ForSourceMember(s => s.Buyer, opt => opt.DoNotValidate()); 

        }
    }
}
