using CodeChallenge.Models;

namespace CodeChallenge.Services;

public interface IReportingStructureService
{
    ReportingStructure GetByEmployeeId(string employeeId);
}
