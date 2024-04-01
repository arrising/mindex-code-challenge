using CodeChallenge.Models;
using CodeChallenge.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Services;

public class ReportingStructureService : IReportingStructureService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeService> _logger;

    public ReportingStructureService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public ReportingStructure GetByEmployeeId(string employeeId)
    {
        if (string.IsNullOrEmpty(employeeId))
        {
            return null;
        }

        var employee = _employeeRepository.GetById(employeeId);
        if (employee == null)
        {
            return null;
        }

        var reportsCount = _employeeRepository.GetReportsCountByEmployeeId(employeeId);

        return new ReportingStructure
        {
            Employee = employee,
            NumberOfReports = reportsCount
        };
    }
}
