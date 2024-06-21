using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;

namespace SoftUni;

    public class StartUp
    {
        static void Main(string[] args)
        {
            SoftUniContext dbContext = new();
            //Console.WriteLine(GetEmployeesFullInformation(dbContext));
            //Console.WriteLine(GetEmployeesWithSalaryOver50000(dbContext));
            //Console.WriteLine(GetEmployeesFromResearchAndDevelopment(dbContext));
            //Console.WriteLine(AddNewAddressToEmployee(dbContext));
            //Console.WriteLine(GetEmployeesInPeriod(dbContext));
            //Console.WriteLine(DeleteProjectById(dbContext));
            //Console.WriteLine(GetAddressesByTown(dbContext));
            //Console.WriteLine(GetEmployee147(dbContext));
            //Console.WriteLine(GetDepartmentsWithMoreThan5Employees(dbContext));
            //Console.WriteLine(GetLatestProjects(dbContext));
            //Console.WriteLine(IncreaseSalaries(dbContext));
            //Console.WriteLine(GetEmployeesByFirstNameStartingWithSa(dbContext));
            //Console.WriteLine(RemoveTown(dbContext));
    }


        // 03 Employees Full Information
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder sb = new();

            var employees = context.Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    e.JobTitle,
                    e.Salary
                })
                .ToArray();

            foreach (var employee in employees)
            {
                sb.AppendLine(
                    $"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 04 Employees with Salary Over 50 000
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder sb = new();
            var highEarners = context.Employees
                .Select(e => new { e.FirstName, e.Salary })
                .Where(e => e.Salary > 50000)
                .OrderBy(e => e.FirstName)
                .ToArray();
            foreach (var he in highEarners)
            {
                sb.AppendLine($"{he.FirstName} - {he.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 05 Employees from Research and Development

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder sb = new();

            var employees = context.Employees
                .Where(e => e.Department.Name == "Research and Development")
                .Select(e => new { e.FirstName, e.LastName, DepartmentName = e.Department.Name, e.Salary })
                .OrderBy(e => e.Salary).ThenByDescending(e => e.FirstName)
                .ToArray();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} from {e.DepartmentName} - ${e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //06 Adding a New Address and Updating Employee

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            StringBuilder sb = new();

            Address newAddress = new()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            Employee? employee = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");
            employee!.Address = newAddress;

            context.SaveChanges();

            var addresses = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText)
                .ToArray();
                

            foreach (var a in addresses)
            {
                sb.AppendLine(a);
            }

            return sb.ToString().TrimEnd(); 
        }

        // 07 Employees and Projects

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder sb = new();

            var employees = context.Employees
                //.Where(e => e.EmployeesProjects.Any(e =>
                //    e.Project.StartDate.Year >= 2001 && e.Project.StartDate.Year <= 2003))
                .Take(10)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    MFirstName = e.Manager.FirstName,
                    MLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects
                        .Where(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003)
                        .Select(ep => new
                    {
                        ProjectName = ep.Project.Name,
                        StartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                        EndDate = ep.Project.EndDate.HasValue
                            ? ep.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                            : "not finished"
                    }).ToArray()
                })
                .ToArray();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.MFirstName} {e.MLastName}");
                foreach (var p in e.Projects)
                {
                    sb.AppendLine($"--{p.ProjectName} - {p.StartDate} - {p.EndDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }


        // 08. Addresses by Town
        public static string GetAddressesByTown(SoftUniContext context)
         {
             StringBuilder sb = new();

             var addresses = context.Addresses
                 .AsNoTracking()
                 .OrderByDescending(a => a.Employees.Count)
                 .ThenBy(a => a.Town.Name)
                 .ThenBy(a => a.AddressText)
                 .Take(10)
                 .Select(a => new
                 {
                     a.AddressText,
                     TownName = a.Town.Name,
                     Count = a.Employees.Count
                 })
                 .ToArray();


             foreach (var a in addresses)
             {
                 sb.AppendLine($"{a.AddressText}, {a.TownName} - {a.Count} employees");
             }
             

             return sb.ToString().TrimEnd();
         }

        // 09. Employee 147

        public static string GetEmployee147(SoftUniContext context)
    {
        StringBuilder sb = new();

        //Employee employee147 = context.Employees.Find(147);

        //sb.AppendLine($"{employee147.FirstName} {employee147.LastName} - {employee147.JobTitle}");


        //string[] projects = employee147.EmployeesProjects.Select(p => p.Project.Name).ToArray();

        var employee147Projects = context.Employees
            .Where(e => e.EmployeeId == 147)
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                e.JobTitle,
                Projects = e.EmployeesProjects.Select(ep => ep.Project.Name).OrderBy(x=>x).ToArray()
            }).ToArray();

        foreach (var ep in employee147Projects)
        {
            sb.AppendLine($"{ep.FirstName} {ep.LastName} - {ep.JobTitle}");
            foreach (var name in ep.Projects)
            {
                sb.AppendLine(name);
            }
        }

        return sb.ToString().TrimEnd();
    }

        // 10. Departments with More Than 5 Employees 
    
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
    {
        StringBuilder sb = new();

        var departments = context.Departments
            .AsNoTracking()
            .Where(d => d.Employees.Count > 5)
            .OrderBy(d => d.Employees.Count)
            .ThenBy(d => d.Name)
            .Select(d => new
            {
                d.Name,
                MfirstName = d.Manager.FirstName,
                MlastName = d.Manager.LastName,
                Employees = d.Employees
                    .Select(e => new
                    {
                        e.FirstName,
                        e.LastName,
                        e.JobTitle
                    }).ToArray()
            }).ToArray();

        foreach (var d in departments)
        {
            sb.AppendLine($"{d.Name} - {d.MfirstName} {d.MlastName}");
            foreach (var e in d.Employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
            }
        }

        return sb.ToString().TrimEnd();
    }

        //11. Find Latest 10 Projects

        public static string GetLatestProjects(SoftUniContext context)
        {
            StringBuilder sb = new();


            var latestProjects = context.Projects.OrderByDescending(lp => lp.StartDate)
                .AsNoTracking()
                .Take(10)
                .Select(lp => new
                {
                    lp.Name,
                    lp.Description,
                    StatDate = lp.StartDate.ToString("M/d/yyyy h:mm:ss tt")
                })
                .OrderBy(lp => lp.Name)
                .ToArray();


            foreach (var p in latestProjects)
            {
                sb.AppendLine(p.Name);
                sb.AppendLine(p.Description);
                sb.AppendLine(p.StatDate);
            }

            return sb.ToString().TrimEnd();
        }

         // 12. Increase Salaries

         public static string IncreaseSalaries(SoftUniContext context)
         {
             StringBuilder sb = new();

             var employess = context.Employees
                 .AsNoTracking()
                 .Where(e => e.Department.Name == "Engineering" || e.Department.Name == "Tool Design"
                                                                || e.Department.Name == "Marketing"
                                                                || e.Department.Name == "Information Services")
                 .OrderBy(e => e.FirstName)
                 .ThenBy(e => e.LastName)
                 .ToArray();

             foreach (var e in employess)
             {
                 e.Salary += (e.Salary / 100) * 12;
                 sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:F2})");
             }

             return sb.ToString().TrimEnd();
         }

        //13. Find Employees by First Name Starting With Sa

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            StringBuilder sb = new();

            var employeesWithSa = context.Employees
                .AsNoTracking()
                .Where(e => e.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToArray();

            foreach (var e in employeesWithSa)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        // 14 Delete Project by Id

        public static string DeleteProjectById(SoftUniContext context)
         {
             IQueryable<EmployeeProject> epToDelete = context.EmployeesProjects
                 .Where(p => p.ProjectId == 2);
             context.EmployeesProjects.RemoveRange(epToDelete);

             Project projectToDelete = context.Projects.Find(2);
             context.Projects.Remove(projectToDelete);

             context.SaveChanges();

             string[] projectNames = context.Projects
                 .Take(10)
                 .Select(p => p.Name)
                 .ToArray();

            return string.Join(Environment.NewLine, projectNames);
         }

        //15. Remove Town 

        public static string RemoveTown(SoftUniContext context)
        {

            var employees = context.Employees
                .Where(e => e.Address.Town.Name == "Seattle");

            foreach (var e in employees)
            {
                e.AddressId = null;
            }

            var addresses = context.Addresses
                .Where(a => a.Town.Name == "Seattle");

            int count = addresses.Count();

            context.Addresses.RemoveRange(addresses);
            Town townToRemove = context.Towns.First(t => t.Name == "Seattle");
            context.Towns.Remove(townToRemove);
            context.SaveChanges();

            return $"{count} addresses in Seattle were deleted";
        }

}

