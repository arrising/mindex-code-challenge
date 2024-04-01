using CodeChallenge.Models;
using CodeChallenge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Controllers;

[ApiController]
[Route("api/employee")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IReportingStructureService _reportingStructureService;
    private readonly ILogger _logger;

    public EmployeeController(ILogger<EmployeeController> logger,
        IEmployeeService employeeService,
        IReportingStructureService reportingStructureService)
    {
        _logger = logger;
        _employeeService = employeeService;
        _reportingStructureService = reportingStructureService;
    }

    [HttpPost]
    public IActionResult CreateEmployee([FromBody] Employee employee)
    {
        _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

        _employeeService.Create(employee);

        return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
    }

    [HttpGet("{id}", Name = "getEmployeeById")]
    public IActionResult GetEmployeeById(string id)
    {
        _logger.LogDebug($"Received employee get request for '{id}'");

        var employee = _employeeService.GetById(id);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpPut("{id}")]
    public IActionResult ReplaceEmployee(string id, [FromBody] Employee newEmployee)
    {
        _logger.LogDebug($"Recieved employee update request for '{id}'");

        var existingEmployee = _employeeService.GetById(id);
        if (existingEmployee == null)
        {
            return NotFound();
        }

        _employeeService.Replace(existingEmployee, newEmployee);

        return Ok(newEmployee);
    }

    [HttpGet("{id}/reportingStructure", Name = "getReportingStructureByEmployeeId")]
    public IActionResult GetReportingStructureByEmployeeId(string id)
    {
        _logger.LogDebug($"Received reporting structure get request for employee '{id}'");

        var reportingStructure = _reportingStructureService.GetByEmployeeId(id);

        if (reportingStructure == null)
        {
            return NotFound();
        }

        return Ok(reportingStructure);
    }
}
