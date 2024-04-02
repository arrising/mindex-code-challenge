using System;
using CodeChallenge.Exceptions;
using CodeChallenge.Models;
using CodeChallenge.Repositories;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Services;

public class CompensationService : ICompensationService
{
    private readonly ICompensationRepository _compensationRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private ILogger<CompensationService> _logger;

    public CompensationService(ILogger<CompensationService> logger, ICompensationRepository compensationRepository,
        IEmployeeRepository employeeRepository)
    {
        _compensationRepository = compensationRepository;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public Compensation Create(CompensationCreateRequest compensation)
    {
        if (compensation == null)
        {
            throw new ArgumentNullException(nameof(compensation));
        }

        if (string.IsNullOrWhiteSpace(compensation.EmployeeId))
        {
            throw new ArgumentNullException(compensation.EmployeeId);
        }

        var employee = _employeeRepository.GetById(compensation.EmployeeId) ?? throw new NotFoundException();
        var existing = _compensationRepository.GetByEmployeeId(compensation.EmployeeId);

        var record = new Compensation
        {
            Salary = compensation.Salary,
            EffectiveDate = compensation.EffectiveDate,
            EmployeeId = compensation.EmployeeId,
            Employee = employee
        };

        return existing == null 
            ? _compensationRepository.Add(record)
            : Replace(existing, record);
    }

    public Compensation GetByEmployeeId(string employeeId) =>
        !string.IsNullOrEmpty(employeeId)
            ? _compensationRepository.GetByEmployeeId(employeeId)
            : null;

    private Compensation Replace(Compensation originalCompensation, Compensation newCompensation)
    {
        if (originalCompensation != null)
        {
            _compensationRepository.Remove(originalCompensation);
            if (newCompensation != null)
            {
                // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                _compensationRepository.SaveAsync().Wait();

                _compensationRepository.Add(newCompensation);
                // overwrite the new id with previous employee id
                newCompensation.EmployeeId = originalCompensation.EmployeeId;
            }

            _employeeRepository.SaveAsync().Wait();
        }

        return newCompensation;
    }
}
