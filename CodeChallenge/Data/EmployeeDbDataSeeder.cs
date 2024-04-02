using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Newtonsoft.Json;

namespace CodeChallenge.Data;

public class EmployeeDbDataSeeder
{
    private const string EMPLOYEE_SEED_DATA_FILE = "resources/EmployeeSeedData.json";
    private const string COMPENSATION_SEED_DATA_FILE = "resources/CompensationSeedData.json";
    private readonly EmployeeDbContext _context;

    public EmployeeDbDataSeeder(EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (!_context.Employees.Any())
        {
            var employees = LoadEmployees();
            _context.Employees.AddRange(employees);

            await _context.SaveChangesAsync();
        }

        if (!_context.Compensation.Any())
        {
            var compensations = LoadCompensation(_context.Employees.ToList());
            _context.Compensation.AddRange(compensations);

            await _context.SaveChangesAsync();
        }
    }

    private List<Employee> LoadEmployees()
    {
        using (var fileStream = new FileStream(EMPLOYEE_SEED_DATA_FILE, FileMode.Open))
        using (var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
            var serializer = new JsonSerializer();

            var employees = serializer.Deserialize<List<Employee>>(jsonReader);
            FixEmployeeReferences(employees);

            return employees;
        }
    }

    private void FixEmployeeReferences(List<Employee> employees)
    {
        var employeeIdRefMap = GetEmployeeRefMap(employees);

        foreach (var employee in employees.Where(x => x.DirectReports != null))
        {
            var referencedEmployees = employee.DirectReports
                .Where(x => employeeIdRefMap.ContainsKey(x.EmployeeId))
                .Select(x => employeeIdRefMap[x.EmployeeId])
                .ToList();
            employee.DirectReports = referencedEmployees;
        }
    }

    private List<Compensation> LoadCompensation(List<Employee> employees)
    {
        using (var fileStream = new FileStream(COMPENSATION_SEED_DATA_FILE, FileMode.Open))
        using (var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
            var serializer = new JsonSerializer();

            var compensations = serializer.Deserialize<List<Compensation>>(jsonReader);

            FixCompensationReferences(compensations, employees);
            return compensations;
        }
    }

    private void FixCompensationReferences(List<Compensation> compensations, List<Employee> employees)
    {
        var employeeMap = GetEmployeeRefMap(employees);

        compensations.ForEach(compensation =>
        {
            if (!string.IsNullOrWhiteSpace(compensation.EmployeeId) &&
                employeeMap.TryGetValue(compensation.EmployeeId, out var value))
            {
                compensation.Employee = value;
            }
        });
    }

    private ImmutableDictionary<string, Employee> GetEmployeeRefMap(List<Employee> employees)
    {
        return employees.ToImmutableDictionary(x => x.EmployeeId, x => x);
    }
}
