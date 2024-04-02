using System;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Data;
using CodeChallenge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Repositories;

public class CompensationRepository : ICompensationRepository
{
    private readonly EmployeeDbContext _context;
    private readonly ILogger<CompensationRepository> _logger;

    public CompensationRepository(ILogger<CompensationRepository> logger, EmployeeDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public Compensation Add(Compensation compensation)
    {
        compensation.CompensationId = Guid.NewGuid().ToString();
        _context.Compensation.Add(compensation);
        _context.SaveChanges();
        return compensation;
    }

    public Compensation GetByEmployeeId(string employeeId)
    {
        return _context.Compensation
            .Include(x => x.Employee)
            .SingleOrDefault(x => x.EmployeeId == employeeId);
    }

    public Compensation Remove(Compensation compensation) => _context.Remove(compensation).Entity;

    public Task SaveAsync()
    {
        return _context.SaveChangesAsync();
    }
}
