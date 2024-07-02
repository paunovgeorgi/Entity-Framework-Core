using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;
using BookShop.Models;
using BookShop.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookShop
{
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            //Console.WriteLine(GetBooksByAgeRestriction(db, "miNor"));
            //Console.WriteLine(GetGoldenBooks(db));
            //Console.WriteLine(GetBooksByPrice(db));
            //Console.WriteLine(GetBooksNotReleasedIn(db, 2000));
            //Console.WriteLine(GetBooksByCategory(db, "horror mystery drama"));
            Console.WriteLine(GetBooksReleasedBefore(db, "30-12-1989"));
            //Console.WriteLine(GetAuthorNamesEndingIn(db, "e"));
            //Console.WriteLine(GetBookTitlesContaining(db, "sK"));
            //Console.WriteLine(GetBooksByAuthor(db, "R"));
            //Console.WriteLine(CountBooks(db, 12));
            //Console.WriteLine(CountCopiesByAuthor(db));
            //Console.WriteLine(GetTotalProfitByCategory(db));
            //Console.WriteLine(GetMostRecentBooks(db));
            //IncreasePrices(db);
            //Console.WriteLine(RemoveBooks(db));
        }

        // 02. Age Restriction

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            //bool hasParsed = Enum.TryParse(typeof(AgeRestriction), command, true, out object? ageRestrictionObj);
            //AgeRestriction ageRestriction;


            //if (hasParsed)
            //{
            //    ageRestriction = (AgeRestriction)ageRestrictionObj;


            //    var bookNames = context.Books
            //        .AsNoTracking()
            //        .Where(b => b.AgeRestriction == ageRestriction)
            //        .Select(b => b.Title)
            //        .OrderBy(b => b)
            //        .ToArray();
            //return String.Join(Environment.NewLine, bookNames);
            //}

            //return null;

            try
            {
                AgeRestriction ageRestriction = Enum.Parse<AgeRestriction>(command, true);
                var bookNames = context.Books
                    .AsNoTracking()
                    .Where(b => b.AgeRestriction == ageRestriction)
                    .Select(b => b.Title)
                    .OrderBy(b => b)
                    .ToArray();
                return String.Join(Environment.NewLine, bookNames);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        // 03. Golden Books

        public static string GetGoldenBooks(BookShopContext context)
        {

            string[] goldenBooks = context.Books
                .Where(b => b.EditionType == EditionType.Gold && b.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

                


            return String.Join(Environment.NewLine, goldenBooks);
        }

        // 04. Books by Price

        public static string GetBooksByPrice(BookShopContext context)
        {

            string[] booksByPrice = context.Books
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .Select(b => $"{b.Title} - ${b.Price:f2}")
                .ToArray();
            

            return string.Join(Environment.NewLine, booksByPrice);
        }

        // 05. Not Released In

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            string[] booksByDate = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)   
                .ToArray();

            return String.Join(Environment.NewLine, booksByDate);
        }

        // 06. Book Titles by Category

        public static string GetBooksByCategory(BookShopContext context, string input)
        {

            string[] categories = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(c => c.ToLower())
                .ToArray();

            var booksByCategory = context.Books
                .Where(b => b.BookCategories.All(bc => categories.Contains(bc.Category.Name.ToLower())))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToArray();
            
            return string.Join(Environment.NewLine, booksByCategory);
        }

        // 07. Released Before Date

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            StringBuilder sb = new();

            string[] parts = date.Split('-');
            string newDate = parts[1] + "-" + parts[0] + "-" + parts[2];

            var books = context.Books
                .Where(b => b.ReleaseDate!.Value.CompareTo(DateTime.Parse(newDate)) < 0)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price
                })
                .ToList();


            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 08. Author Search

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {

            string[] authors = context.Authors
                .Where(a => a.FirstName.EndsWith(input))
                .OrderBy(a => a.FirstName)
                .ThenBy( a => a.LastName)
                .Select(a => $"{a.FirstName} {a.LastName}")
                .ToArray();


            return string.Join(Environment.NewLine, authors);
        }

        // 09. Book Search

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            string[] books = context.Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        // 10. Book Search by Author

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            string[] books = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .Select(b => $"{b.Title} ({b.Author.FirstName} {b.Author.LastName})")
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        // 11. Count Books

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            return context.Books
                .Where(b => b.Title.Length > lengthCheck)
                .Count();
        }

        // 12. Total Book Copies

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            StringBuilder sb = new();

            var totalCopies = context.Authors
                .Select(a => new 
                {
                    Name = $"{a.FirstName} {a.LastName}",
                    Copies = a.Books.Sum(b => b.Copies)

                })
                .OrderByDescending(a => a.Copies)
                .ToList();

            foreach (var copy in totalCopies)
            {
                sb.AppendLine($"{copy.Name} - {copy.Copies}");
            }

            return sb.ToString().TrimEnd();
        }

        // 13. Profit by Category

        public static string GetTotalProfitByCategory(BookShopContext context)
        {

            StringBuilder sb = new();

            var totalByProfit = context.Categories
                //.GroupBy(b => b.Price * b.Copies)
                .Select(c => new
                {
                    c.Name,
                    TotalPrice = c.CategoryBooks.Sum(cb => cb.Book.Price * cb.Book.Copies)
                })
                .OrderByDescending(cb => cb.TotalPrice)
                .ToArray();

            foreach (var tp in totalByProfit)
            {
                sb.AppendLine($"{tp.Name} ${tp.TotalPrice}");
            }

            return sb.ToString().TrimEnd();
        }

        // 14. Most Recent Books

        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder sb = new();

            var recentBooks = context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Name,
                    RecentBooks = c.CategoryBooks.OrderByDescending(b => b.Book.ReleaseDate)
                        .Select(b => new
                        {
                            b.Book.Title,
                            Date = b.Book.ReleaseDate.Value.Year
                        })
                        .Take(3)
                        .ToList()
                })
                .ToList();


            foreach (var bookCategory in recentBooks)
            {
                sb.AppendLine($"--{bookCategory.Name}");
                foreach (var book in bookCategory.RecentBooks)
                {
                    sb.AppendLine($"{book.Title} ({book.Date})");
                }
            }   

            return sb.ToString().TrimEnd();
        }

        // 15. Increase Prices

        public static void IncreasePrices(BookShopContext context)
        {
            List<Book> books = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (Book book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();

        }

        // 16. Remove Books

        public static int RemoveBooks(BookShopContext context)
        {
            var bookstToRemove = context.Books
                .Where(b => b.Copies < 4200)
                .ToArray();

            int result = bookstToRemove.Length;
            context.RemoveRange(bookstToRemove);
            context.SaveChanges();

            return result;
        }
    }
}


