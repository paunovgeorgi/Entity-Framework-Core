using System.Collections;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using ProductShop.Utilities;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            using ProductShopContext db = new ProductShopContext();

            string usersInputXml = File.ReadAllText(@"../../../Datasets/users.xml");
            string productsInputXml = File.ReadAllText(@"../../../Datasets/products.xml");
            string categoriesInputXml = File.ReadAllText(@"../../../Datasets/categories.xml");
            string categoryProductsInputXml = File.ReadAllText(@"../../../Datasets/categories-products.xml");

            //db.Database.EnsureDeleted();
            //db.Database.EnsureCreated();

            //Console.WriteLine(ImportUsers(db, usersInputXml));
            //Console.WriteLine(ImportProducts(db, productsInputXml));
            //Console.WriteLine(ImportCategories(db, categoriesInputXml));
            //Console.WriteLine(ImportCategoryProducts(db, categoryProductsInputXml));

            //Console.WriteLine(GetProductsInRange(db));
            //Console.WriteLine(GetSoldProducts(db));
            //Console.WriteLine(GetCategoriesByProductsCount(db));
            Console.WriteLine(GetUsersWithProducts(db));

        }


        // 01. Import Users

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ImportUserDto[] userDtos = xmlHelper.Deserialize<ImportUserDto[]>(inputXml, "Users");
            User[] users = mapper.Map<User[]>(userDtos);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        // 02. Import Products

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ImportProductDto[] productDtos = xmlHelper.Deserialize<ImportProductDto[]>(inputXml, "Products");
            Product[] products = mapper.Map<Product[]>(productDtos);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        // 03. Import Categories 

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            IMapper mapper = CreateMapper();
            XmlHelper xmlHelper = new XmlHelper();

            ImportCategoryDto[] categoryDtos = xmlHelper.Deserialize<ImportCategoryDto[]>(inputXml, "Categories");
            Category[] categories = mapper.Map<Category[]>(categoryDtos);

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        // 04. Import Categories and Products

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            int[] categoryIds = context.Categories.Select(c => c.Id).ToArray();
            int[] productIds = context.Products.Select(p => p.Id).ToArray();
            XmlHelper xmlHelper = new XmlHelper();
            IMapper mapper = CreateMapper();
            ICollection<CategoryProduct> categoryProducts = new List<CategoryProduct>();

            ImportCategoryProductDto[]
                cpDtos = xmlHelper.Deserialize<ImportCategoryProductDto[]>(inputXml, "CategoryProducts");

            foreach (ImportCategoryProductDto cpDto in cpDtos)
            {
                if (!categoryIds.Any(x => x == cpDto.CategoryId) || !productIds.Any(x => x == cpDto.ProductId))
                {
                    continue;
                }

                CategoryProduct cp = mapper.Map<CategoryProduct>(cpDto);
                categoryProducts.Add(cp);
            }

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Count}";
        }

        // 05. Export Products In Range 

        public static string GetProductsInRange(ProductShopContext context)
        {

            XmlHelper xmlHelper = new XmlHelper();
            IMapper mapper = CreateMapper();
            ExportProductDto[] productDtos = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .AsNoTracking()
                .OrderBy(p => p.Price)
                .Take(10)
                .ProjectTo<ExportProductDto>(mapper.ConfigurationProvider)
                .ToArray();

            return xmlHelper.Serialize<ExportProductDto[]>(productDtos, "Products");
        }

        // 06. Export Sold Products

        public static string GetSoldProducts(ProductShopContext context)
        {
            XmlHelper xmlHelper = new XmlHelper();

            ExportUserDto[] userDtos = context.Users
                .Where(u => u.ProductsSold.Count > 0)
                .AsNoTracking()
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .Select(u => new ExportUserDto()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold.Select(ps => new SoldProductDto()
                    {
                        Name = ps.Name,
                        Price = ps.Price
                    }).ToArray()
                })
                .ToArray();

            return xmlHelper.Serialize<ExportUserDto[]>(userDtos, "Users");
        }

        // 07. Export Categories By Products Count

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            XmlHelper xmlHelper = new XmlHelper();

            ExportCategoryDto[] categoryDtos = context.Categories
                .Select(c => new ExportCategoryDto()
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToArray();

            return xmlHelper.Serialize<ExportCategoryDto[]>(categoryDtos, "Categories");
        }

        // 08. Export Users and Products

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            XmlHelper xmlHelper = new XmlHelper();

            ExportUserMainDto08 outputMain = new ExportUserMainDto08()
            {
                Count = context.Users.Count(u => u.ProductsSold.Count > 0),
                Users = context.Users
                    .Where(u => u.ProductsSold.Count > 0)
                    .OrderByDescending(u => u.ProductsSold.Count)
                    .Take(10)
                    .Select(u => new ExportUserDtro08()
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Age = u.Age,
                        SoldProducts = new ExportSoldProductDto08()
                        {
                            Count = u.ProductsSold.Count,
                            Products = u.ProductsSold.Select(cp => new ExportProductDto08()
                                {
                                    Name = cp.Name,
                                    Price = cp.Price
                                })
                                .OrderByDescending(x => x.Price)
                                .ToArray()
                        }
                    })
                    .ToArray()
            };

            //ExportUserDtro08[] usersDtos = context.Users
            //    .Where(u => u.ProductsSold.Count > 0)
            //    .OrderByDescending(u => u.ProductsSold.Count)
            //    .Take(10)
            //    .Select(u => new ExportUserDtro08()
            //    {
            //        FirstName = u.FirstName,
            //        LastName = u.LastName,
            //        Age = u.Age,
            //        SoldProducts = new ExportSoldProductDto08()
            //        {
            //            Count = u.ProductsSold.Count,
            //            Products = u.ProductsSold.Select(cp => new ExportProductDto08()
            //            {
            //                Name = cp.Name,
            //                Price = cp.Price
            //            })
            //            .OrderByDescending(x => x.Price)
            //            .ToArray()
            //        } 
            //    })
            //    .ToArray();


            return xmlHelper.Serialize<ExportUserMainDto08>(outputMain, "Users");
        }


        private static IMapper CreateMapper() =>
            new Mapper(new MapperConfiguration(cfg => 
                cfg.AddProfile<ProductShopProfile>()));
    }
}