using System.Collections;
using System.Net.Mime;
using System.Text;
using System.Xml.Serialization;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using CarDealer.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            using CarDealerContext db = new CarDealerContext();

            string suppliersXmlInput = File.ReadAllText(@"../../../Datasets/suppliers.xml");
            string partsXmlInput = File.ReadAllText(@"../../../Datasets/parts.xml");
            string carsXmlInput = File.ReadAllText(@"../../../Datasets/cars.xml");
            string customersXmlInput = File.ReadAllText(@"../../../Datasets/customers.xml");
            string salesXmlInput = File.ReadAllText(@"../../../Datasets/sales.xml");

            //db.Database.EnsureDeleted();
            //db.Database.EnsureCreated();

            //Console.WriteLine(ImportSuppliers(db, suppliersXmlInput));
            //Console.WriteLine(ImportParts(db, partsXmlInput));
            //Console.WriteLine(ImportCars(db, carsXmlInput));
            //Console.WriteLine(ImportCustomers(db, customersXmlInput));
            //Console.WriteLine(ImportSales(db, salesXmlInput));
            //Console.WriteLine(GetCarsWithDistance(db));
            //Console.WriteLine(GetCarsFromMakeBmw(db));
            //Console.WriteLine(GetLocalSuppliers(db));
            //Console.WriteLine(GetCarsWithTheirListOfParts(db));
            //Console.WriteLine(GetTotalSalesByCustomer(db));
            Console.WriteLine(GetSalesWithAppliedDiscount(db));
        }


        // 09. Import Suppliers

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            IMapper mapper = CreateMapper();

            XmlHelper xmlHelper = new XmlHelper();
            ImportSupplierDto[] supplierDtos = xmlHelper.Deserialize<ImportSupplierDto[]>(inputXml, "Suppliers");

            //ICollection<Supplier> validSuppliers = new HashSet<Supplier>();
            //foreach (ImportSupplierDto supplierDto in supplierDtos)
            //{
            //    if (String.IsNullOrEmpty(supplierDto.Name))
            //    {
            //        continue;
            //    }

            //    Supplier supplier = new Supplier()
            //    {
            //        Name = supplierDto.Name,
            //        IsImporter = supplierDto.IsImporter
            //    };

            //    validSuppliers.Add(supplier);
            //}

            Supplier[] suppliers = mapper.Map<Supplier[]>(supplierDtos);

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}";
        }

        // 10. Import Parts 

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            int[] supplierIds = context.Suppliers.Select(s => s.Id).ToArray();
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ImportPartDto[] partsDtos = xmlHelper.Deserialize<ImportPartDto[]>(inputXml, "Parts");

            ICollection<Part> validParts = new HashSet<Part>();

            foreach (ImportPartDto part in partsDtos)
            {
                if (!supplierIds.Any(s => s == part.SupplierId))
                {
                    continue;
                }

                Part validPart = mapper.Map<Part>(part);
                validParts.Add(validPart);
            }

            context.Parts.AddRange(validParts);
            context.SaveChanges();

            return $"Successfully imported {validParts.Count}";
        }

        //  11. Import Cars 

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ImportCarDto[] carDtos = xmlHelper.Deserialize<ImportCarDto[]>(inputXml, "Cars");

            ICollection<Car> validCars = new HashSet<Car>();
            ICollection<PartCar> partCars = new List<PartCar>();

            foreach (ImportCarDto carDto in carDtos)
            {
                Car car = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TraveledDistance = carDto.TraveledDistance
                };

                foreach (ImportCarPartDto partDto in carDto.Parts.DistinctBy(p => p.PartId))
                {
                    if (!context.Parts.Any(p => p.Id == partDto.PartId))
                    {
                        continue;
                    }

                    PartCar part = new PartCar()
                    {
                        Car = car,
                        PartId = partDto.PartId
                    };
                    car.PartsCars.Add(part);
                   // partCars.Add(part);
                }

                validCars.Add(car);
            }

            context.Cars.AddRange(validCars);
            //context.PartsCars.AddRange(partCars);
            context.SaveChanges();

            return $"Successfully imported {validCars.Count}";
        }

        // 12. Import Customers

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ImportCustomerDto[] customerDtos = xmlHelper.Deserialize<ImportCustomerDto[]>(inputXml, "Customers");
            ICollection<Customer> validCustomers = new List<Customer>();

            foreach (ImportCustomerDto customerDto in customerDtos)
            {
                if (String.IsNullOrEmpty(customerDto.Name))
                {
                    continue;
                }

                Customer customer = mapper.Map<Customer>(customerDto);
                validCustomers.Add(customer);
            }

            context.Customers.AddRange(validCustomers);
            context.SaveChanges();

            return $"Successfully imported {validCustomers.Count}";
        }

        // 13. Import Sales

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ImportSaleDto[] salesDtos = xmlHelper.Deserialize<ImportSaleDto[]>(inputXml, "Sales");
            ICollection<Sale> validSales = new List<Sale>();

            foreach (ImportSaleDto saleDto in salesDtos)
            {
                if (!context.Cars.Any(c => c.Id == saleDto.CarId))
                {
                    continue;
                }

                Sale sale = mapper.Map<Sale>(saleDto);
                validSales.Add(sale);
            }

            context.Sales.AddRange(validSales);
            context.SaveChanges();

            return $"Successfully imported {validSales.Count}";
        }

        // 14. Export Cars With Distance

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ExportCarDto[] cars = context.Cars
                .Where(c => c.TraveledDistance > 2000000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .ProjectTo<ExportCarDto>(mapper.ConfigurationProvider)
                .ToArray();

            return xmlHelper.Serialize<ExportCarDto[]>(cars, "cars");

        }

        // 15. Export Cars From Make BMW

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ExportCarAttributesDto[] bmwCars = context.Cars
                .AsNoTracking()
                .Where(c => c.Make == "BMW")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .ProjectTo<ExportCarAttributesDto>(mapper.ConfigurationProvider)
                .ToArray();

            return xmlHelper.Serialize<ExportCarAttributesDto[]>(bmwCars, "cars");
        }

        // 16. Export Local Suppliers

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ExportSupplierDto[] supplierDtos = context.Suppliers
                .AsNoTracking()
                .Where(s => !s.IsImporter)
                .ProjectTo<ExportSupplierDto>(mapper.ConfigurationProvider)
                .ToArray();

            return xmlHelper.Serialize<ExportSupplierDto[]>(supplierDtos, "suppliers");
        }

        // 17. Export Cars With Their List Of Parts 

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            var carsWithParts = context.Cars
                .AsNoTracking()
                .OrderByDescending(c => c.TraveledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .Select(c => new ExportCarDto17()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                    Parts = c.PartsCars.Select(pc => new ExportPartDto()
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price,
                    })
                    .OrderByDescending(x => x.Price)
                    .ToArray()
                })
                .ToArray();

            return xmlHelper.Serialize(carsWithParts, "cars");
        }

        // 18. Export Total Sales By Customer

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            XmlHelper xmlHelper = new XmlHelper();

            var totalSalesWithArr = context.Customers
                .Where(c => c.Sales.Count > 0)
                .Select(c => new
                {
                    c.Name,
                    BoughtCars = c.Sales.Count,
                    IsYoung = c.IsYoungDriver,
                    SpentMoneyArr = c.Sales.Select(s => s.Car.PartsCars.Sum(pc => pc.Part.Price)).ToArray()
                })
                .ToArray();

            ICollection<ExportCustomerDto> customerTotalSales = new List<ExportCustomerDto>();

            foreach (var customer in totalSalesWithArr)
            {
                ExportCustomerDto exportCustomer = new ExportCustomerDto()
                {
                    Name = customer.Name,
                    BoughtCars = customer.BoughtCars,
                    SpentMoney = customer.IsYoung
                        ? customer.SpentMoneyArr.Sum() * (decimal)0.95
                        : customer.SpentMoneyArr.Sum()
                };

                customerTotalSales.Add(exportCustomer);
            }

            ExportCustomerDto[] output = customerTotalSales.OrderByDescending(c => c.SpentMoney)
                .Select(c => new ExportCustomerDto()
                {
                    Name = c.Name,
                    BoughtCars = c.BoughtCars,
                    SpentMoney = Math.Round(c.SpentMoney, 2, MidpointRounding.ToZero)
                })
                .ToArray();

            return xmlHelper.Serialize<ExportCustomerDto[]>(output, "customers");
        }

        // 19. Export Sales With Applied Discount

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            XmlHelper xmlHelper = new XmlHelper();

            ExportSaleDto[] salesWithDiscountDto = context.Sales
                .Select(s => new ExportSaleDto()
                {
                    Car = new ExportCarDto19()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TraveledDistance
                    },
                    Discount = s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = (double)s.Car.PartsCars.Sum(pc => pc.Part.Price),
                    PriceWithDiscount = (double)(s.Car.PartsCars.Sum(pc => pc.Part.Price) - 
                                        (s.Car.PartsCars.Sum(pc => pc.Part.Price) *  (s.Discount / 100)))
                })
                .ToArray();

            return xmlHelper.Serialize<ExportSaleDto[]>(salesWithDiscountDto, "sales");
        }

        private static IMapper CreateMapper()
            => new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<CarDealerProfile>()));


    }
}