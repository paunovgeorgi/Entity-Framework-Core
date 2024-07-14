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
            CreateMap<ImportUserDto, User>();

            CreateMap<ImportProductDto, Product>();



            // Product
            CreateMap<ImportCategoryDto, Category>();

            CreateMap<Product, ExportProductInRangeDto>()
                .ForMember(d => d.Seller,
                    opt => opt.MapFrom(s => $"{s.Seller.FirstName} {s.Seller.LastName}"));

            CreateMap<Product, ExportSoldProductDto>();

            CreateMap<ExportSoldProductDto, ExportSoldProductUserDto>();

            CreateMap<User, ExportSoldProductUserDto>();
            
   





            CreateMap<ImportCategoryProductDto, CategoryProduct>();
        }
    }
}
