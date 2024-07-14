using AutoMapper;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            CreateMap<ImportSupplierDto, Supplier>();

            CreateMap<ImportPartDto, Part>();

            CreateMap<ImportCarDto, Car>();

            CreateMap<ImportCustomerDto, Customer>();

            CreateMap<ImportSaleDto, Sale>();

            // Export

            CreateMap<Car, ExportCarDto>();
            CreateMap<Car, ExportCarAttributesDto>();

            CreateMap<Supplier, ExportSupplierDto>()
                .ForMember(d => d.PartsCount, opt => opt.MapFrom(
                    s => s.Parts.Count));

            CreateMap<Customer, ExportCustomerDto>()
                .ForMember(d => d.BoughtCars, opt => opt
                    .MapFrom(s => s.Sales.Count));


            CreateMap<Car, ExportCarDto19>();



        }
    }
}
