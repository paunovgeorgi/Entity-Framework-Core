using AutoMapper;
using CarDealer.DTOs.Import;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            CreateMap<ImportSupplierDto, Supplier>();

            CreateMap<ImportCarDto, Car>()
                .ForMember(d => d.PartsCars, opt => opt.MapFrom(s => s.PartsId));
        }
    }
}
