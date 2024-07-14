using System.Runtime.InteropServices.ComTypes;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
           string usersInputJson = File.ReadAllText(@"../../../Datasets/users.json");
           string productsInputJson = File.ReadAllText(@"../../../Datasets/products.json");
           string categoriesInputJson = File.ReadAllText(@"../../../Datasets/categories.json");
           string categoriesProductsInputJson = File.ReadAllText(@"../../../Datasets/categories-products.json");


            ProductShopContext context = new ProductShopContext();

           //context.Database.EnsureDeleted();
           //context.Database.EnsureCreated();


           //Console.WriteLine(ImportUsers(context, usersInputJson));
           //Console.WriteLine(ImportProducts(context, productsInputJson));
           //Console.WriteLine(ImportCategories(context, categoriesInputJson));
           //Console.WriteLine(ImportCategoryProducts(context, categoriesProductsInputJson));
           //Console.WriteLine(GetProductsInRange(context));
           //Console.WriteLine(GetSoldProducts(context));
           //Console.WriteLine(GetCategoriesByProductsCount(context));
           Console.WriteLine(GetUsersWithProducts(context));


        }

        // 01. Import Users

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            IMapper mapper = CreateMapper();

            var usersDto = JsonConvert.DeserializeObject<ImportUserDto[]>(inputJson);

            //ICollection<User> validUsers = new HashSet<User>();

            //foreach (ImportUserDto userDto in usersDto!)
            //{
            //    User user = mapper.Map<User>(userDto);
            //    validUsers.Add(user);
            //}

            //context.Users.AddRange(validUsers);

            User[] users = mapper.Map<User[]>(usersDto);
            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        // 02. Import Products

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            IMapper mapper = CreateMapper();

            ImportProductDto[] productDtos = JsonConvert.DeserializeObject<ImportProductDto[]>(inputJson);

            Product[] products = mapper.Map<Product[]>(productDtos);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        // 03. Import Categories

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            IMapper mapper = CreateMapper();

            var categoryDtos = JsonConvert.DeserializeObject<ImportCategoryDto[]>(inputJson);

            //Category[] categories = mapper.Map<Category[]>(categoryDtos);

            var validCategories = new List<Category>();

            foreach (var category in categoryDtos)
            {
                if (!String.IsNullOrEmpty(category.Name))
                {
                    Category currentCategory = mapper.Map<Category>(category);
                    validCategories.Add(currentCategory);
                }
            }

            context.Categories.AddRange(validCategories);
            context.SaveChanges();

            return $"Successfully imported {validCategories.Count}";
        }

        // 04. Import Categories and Products

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {

            IMapper mapper = CreateMapper();

            var categoryProductDtos = JsonConvert.DeserializeObject<ImportCategoryProductDto[]>(inputJson);
            CategoryProduct[] categoryProducts = mapper.Map<CategoryProduct[]>(categoryProductDtos);

            context.CategoriesProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Length}";
        }

        // 05. Export Products In Range

        public static string GetProductsInRange(ProductShopContext context)
        {
            IMapper mapper = CreateMapper();

            //var productsDtos = context.Products
            //    .Where(p => p.Price >= 500 && p.Price <= 1000)
            //    .OrderBy(p => p.Price)
            //    .AsNoTracking()
            //    .Select(p => new
            //    {
            //        name = p.Name,
            //        price = p.Price,
            //        seller = p.Seller.FirstName + ' ' + p.Seller.LastName
            //    })
            //    .ToList();

            var productsDtos = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .AsNoTracking()
                .ProjectTo<ExportProductInRangeDto>(mapper.ConfigurationProvider)
                .ToList();

            var productsJson = JsonConvert.SerializeObject(productsDtos, Formatting.Indented);

            return productsJson;
        }

        // 06. Export Sold Products

        public static string GetSoldProducts(ProductShopContext context)
        {
            IContractResolver contractResolver = ConfigureCamelCaseNaming();

            var users = context.Users
                .Where(u => u.ProductsSold.Any(b => b.Buyer != null))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .AsNoTracking()
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    soldProducts = u.ProductsSold.Select(ps => new
                    {
                        name = ps.Name,
                        price = ps.Price,
                        buyerFirstName = ps.Buyer.FirstName,
                        buyerLastName = ps.Buyer.LastName
                    })
                    .ToArray()
                })
                .ToList();


            return JsonConvert.SerializeObject(users, Formatting.Indented, new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            });
        }

        // 07. Export Categories By Products Count

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {

            var categoriesByCount = context.Categories
                .OrderByDescending(c => c.CategoriesProducts.Count)
                .AsNoTracking()
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoriesProducts.Count,
                    averagePrice = /*$"{c.CategoriesProducts.Sum(c => c.Product.Price) / c.CategoriesProducts.Count}:f2"*/
                                   (c.CategoriesProducts.Any() ? c.CategoriesProducts.Average(cp => cp.Product.Price) : 0).ToString("F2"),
                    totalRevenue = (c.CategoriesProducts.Any() ? c.CategoriesProducts.Sum(cp => cp.Product.Price) : 0).ToString("F2")
                })
                .ToList();

            return JsonConvert.SerializeObject(categoriesByCount, Formatting.Indented);
        }

        // 08. Export Users and Products

        public static string GetUsersWithProducts(ProductShopContext context)
        {

            IContractResolver contractResolver = ConfigureCamelCaseNaming();

            var users = context.Users
                .AsNoTracking()
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                //.OrderByDescending(u => u.ProductsSold.Count)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Count(p => p.Buyer != null),
                        products = u.ProductsSold
                            .Where(ps => ps.Buyer != null)
                            .Select(ps => new
                        {
                            name = ps.Name,
                            price = ps.Price
                        }).ToArray()
                    }

                })
                .OrderByDescending(u => u.soldProducts.count)
                .ToList();




            var newUsers = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    u.Age,
                    soldProducts = u.ProductsSold.Where(p => p.BuyerId != null)
                        .Select(ps => new
                        {
                            name = ps.Name,
                            price = ps.Price
                        })
                        .ToList()
                })
                .OrderByDescending(x => x.soldProducts.Count)
                .ToList();


            var wrappedUsersV2 = new
            {
                UsersCount = newUsers.Count,
                Users = newUsers.Select(nw => new
                {
                    nw.FirstName,
                    nw.LastName,
                    nw.Age,
                    soldProducts = new
                    {
                        count = nw.soldProducts.Count,
                        products = nw.soldProducts
                    }

                })
            };



            var userWrappedDt0 = new
            {
                UsersCount = users.Count,
                Users = users
            };

   
            return JsonConvert.SerializeObject(wrappedUsersV2, Formatting.Indented, new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static IMapper CreateMapper()
        {
            return new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            }));
        }

        private static IContractResolver ConfigureCamelCaseNaming()
        {
            return new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy(false, true)
            };
        }
    }
}