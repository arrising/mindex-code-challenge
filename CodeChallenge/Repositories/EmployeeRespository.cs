using System;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CodeChallenge.Data;

namespace CodeChallenge.Repositories
{
    public class EmployeeRespository : IEmployeeRepository
    {
        private readonly EmployeeDbContext _employeeContext;
        private readonly ILogger<IEmployeeRepository> _logger;

        public EmployeeRespository(ILogger<IEmployeeRepository> logger, EmployeeDbContext employeeContext)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public Employee Add(Employee employee)
        {
            employee.EmployeeId = Guid.NewGuid().ToString();
            _employeeContext.Employees.Add(employee);
            return employee;
        }

        public Employee GetById(string id)
        {
            return _employeeContext.Employees.SingleOrDefault(e => e.EmployeeId == id);
        }

        public int GetReportsCountByEmployeeId(string id)
        {
            var employeeAndReports = _employeeContext.Employees
                .Include(employee => employee.DirectReports)
                .ThenInclude(reports => reports.DirectReports)
                .SingleOrDefault(e => e.EmployeeId == id);

            return employeeAndReports != null ? employeeAndReports.DirectReports.Sum(GetReportsCountRecursive) : 0;
        }

        private int GetReportsCountRecursive(Employee employee)
        {
            // Set to 1 to include the employee itself in the count
            var count = 1; 

            if (employee.DirectReports != null)
            {
                count += employee.DirectReports.Sum(GetReportsCountRecursive);
            }

            return count;
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }

        public Employee Remove(Employee employee)
        {
            return _employeeContext.Remove(employee).Entity;
        }
    }
}
