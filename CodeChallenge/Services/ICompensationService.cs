using CodeChallenge.Models;

namespace CodeChallenge.Services;

public interface ICompensationService
{
    public Compensation Create(CompensationCreateRequest compensation);
    public Compensation GetByEmployeeId(string employeeId);
}
