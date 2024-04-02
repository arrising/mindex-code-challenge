using System;
using CodeChallenge.Exceptions;
using CodeChallenge.Models;
using CodeChallenge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Controllers;

[ApiController]
[Route("api/compensation")]
public class CompensationController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly ICompensationService _service;

    public CompensationController(ILogger<EmployeeController> logger,
        ICompensationService compensationService)
    {
        _logger = logger;
        _service = compensationService;
    }

    [HttpPost]
    public IActionResult CreateCompensation([FromBody] CompensationCreateRequest compensation)
    {
        _logger.LogDebug($"Received compensation create request for employee Id'{compensation?.EmployeeId}'");

        try
        {
            var result = _service.Create(compensation);

            return CreatedAtRoute("getByEmployeeId", new { id = result.EmployeeId }, result);
        }
        catch (NotFoundException)
        {
            return NotFound($"EmployeeId '{compensation?.EmployeeId}' does not exist");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating Compensation");
            return BadRequest(exception);
        }
    }

    [HttpGet("{id}", Name = "getByEmployeeId")]
    public IActionResult GetByEmployeeId(string id)
    {
        _logger.LogDebug($"Received employee get request for '{id}'");

        var result = _service.GetByEmployeeId(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
