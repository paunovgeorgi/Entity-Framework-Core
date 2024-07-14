using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            string suppliersJsonInput = File.ReadAllText(@"../../../Datasets/suppliers.json");
            string partsJsonInput = File.ReadAllText(@"../../../Datasets/parts.json");
            string carsJsonInput = File.ReadAllText(@"../../../Datasets/cars.json");
            string customersJsonInput = File.ReadAllText(@"../../../Datasets/customers.json");
            string salesJsonInput = File.ReadAllText(@"../../../Datasets/sales.json");

            using CarDealerContext db = new CarDealerContext();

            //db.Database.EnsureDeleted();
            //db.Database.EnsureCreated();

            //Console.WriteLine(ImportSuppliers(db, suppliersJsonInput));
            //Console.WriteLine(ImportParts(db, partsJsonInput));
            //Console.WriteLine(ImportCars(db, carsJsonInput));
            //Console.WriteLine(ImportCustomers(db, customersJsonInput));
            //Console.WriteLine(ImportSales(db, salesJsonInput));
            //Console.WriteLine(GetOrderedCustomers(db));
            //Console.WriteLine(GetCarsFromMakeToyota(db));
            //Console.WriteLine(GetLocalSuppliers(db));
            //Console.WriteLine(GetCarsWithTheirListOfParts(db));
            //Console.WriteLine(GetTotalSalesByCustomer(db));
            Console.WriteLine(GetSalesWithAppliedDiscount(db));


        }


        // 09. Import Suppliers 
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            IMapper mapper = CreateMapper();
            
            //Supplier[] suppliers = JsonConvert.DeserializeObject<Supplier[]>(inputJson);

            var supplierDtos = JsonConvert.DeserializeObject<ImportSupplierDto[]>(inputJson);
            Supplier[] suppliers = mapper.Map<Supplier[]>(supplierDtos);

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}.";
        }

        // 10. Import Parts 

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            int[] supplierIds = context.Suppliers.Select(s => s.Id).ToArray();
            Part[] parts = JsonConvert.DeserializeObject<Part[]>(inputJson);
            Part[] validParts = parts.Where(p => supplierIds.Contains(p.SupplierId)).ToArray();

            context.Parts.AddRange(validParts);
            context.SaveChanges();

            return $"Successfully imported {validParts.Length}.";
        }

        // 11. Import Cars

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            IMapper mapper = CreateMapper();

            ImportCarDto[] carsDto = JsonConvert.DeserializeObject<ImportCarDto[]>(inputJson);
            List<Car> cars = new List<Car>();
            List<PartCar> partCars = new List<PartCar>();
  

            foreach (var dto in carsDto)
            {
                Car car = new Car();
                car.Model = dto.Model;
                car.Make = dto.Make;
                car.TraveledDistance = dto.TraveledDistance;
                foreach (int id in dto.PartsId.Distinct())
                {
                    PartCar  partCar = new PartCar()
                    {
                        PartId = id,
                        Car = car
                    };
                partCars.Add(partCar);
                }

                cars.Add(car);
            }

            //context.Cars.AddRange(cars);
            context.PartsCars.AddRange(partCars);
            context.SaveChanges();
            
            return $"Successfully imported {cars.Count}.";
        }

        // 12. Import Customers

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            Customer[] customers = JsonConvert.DeserializeObject<Customer[]>(inputJson);
            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}.";
        }

        // 13. Import Sales 

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            Sale[] sales = JsonConvert.DeserializeObject<Sale[]>(inputJson);

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Length}.";
        }

        // 14. Export Ordered Customers

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .AsNoTracking()
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new
                {
                    c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    c.IsYoungDriver
                })
                .ToList();

            return JsonConvert.SerializeObject(customers, Formatting.Indented);
        }

        // 15. Export Cars From Make Toyota

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var toyotaCars = context.Cars
                .AsNoTracking()
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .Select(c => new
                {
                    c.Id,
                    c.Make,
                    c.Model,
                    c.TraveledDistance

                })
                .ToArray();
            
            return JsonConvert.SerializeObject(toyotaCars, Formatting.Indented);
        }

        // 16. Export Local Suppliers 

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var localSuppliers = context.Suppliers
                .Where(s => !s.IsImporter)
                .AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToArray();

            return JsonConvert.SerializeObject(localSuppliers, Formatting.Indented);
        }

        // 17. Export Cars With Their List Of Parts

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsWithParts = context.Cars
                .AsNoTracking()
                .Select(c => new
                {
                    car = new
                    {
                        c.Make,
                        c.Model,
                        c.TraveledDistance
                    },
                    parts = c.PartsCars.Select(pc => new
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price.ToString("F2"),

                    }).ToArray()
                })
                .ToArray();

            return JsonConvert.SerializeObject(carsWithParts, Formatting.Indented);
        }

        // 18. Export Total Sales By Customer 

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var salesByCustomer = context.Customers
                .Where(c => c.Sales.Count > 0)
                .Select(c => new
                {
                    c.Name,
                    BoughtCars = c.Sales.Count,
                    SpentMoney = c.Sales.Select(c => c.Car.PartsCars.Sum(pc => pc.Part.Price)).ToList()
                })
                .ToArray();

            var output = salesByCustomer
                .Select(sbc => new
                {
                    fullName = sbc.Name,
                    boughtCars = sbc.BoughtCars,
                    spentMoney = sbc.SpentMoney.Sum()
                })
                .OrderByDescending(x => x.spentMoney)
                .ThenByDescending(x => x.boughtCars)
                .ToArray();

            return JsonConvert.SerializeObject(output, Formatting.Indented);
        }

        // 19. Export Sales With Applied Discount

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .AsNoTracking()
                .Select(s => new
                {
                    car = new
                    {
                        s.Car.Make,
                        s.Car.Model,
                        s.Car.TraveledDistance
                    },
                    customerName = s.Customer.Name,
                    discount = s.Discount.ToString("F2"),
                    price = s.Car.PartsCars.Sum(pc => pc.Part.Price).ToString("F2"),
                    priceWithDiscount = (s.Car.PartsCars.Sum(pc => pc.Part.Price) * ((100 - s.Discount) / 100)).ToString("F2")
                })
                .Take(10)
                .ToArray();

            return JsonConvert.SerializeObject(sales, Formatting.Indented);
        }


        private static IMapper CreateMapper()
        {
            return new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            }));
        }
        private static IContractResolver CamelCaseFormat()
        {
            return new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy(false, true)
            };
        }
    }
}