using System.Threading.Tasks;
using CodeChallenge.Models;

namespace CodeChallenge.Repositories;

public interface IEmployeeRepository
{
    Employee GetById(string id);
    int GetReportsCountByEmployeeId(string id);
    Employee Add(Employee employee);
    Employee Remove(Employee employee);
    Task SaveAsync();
}
