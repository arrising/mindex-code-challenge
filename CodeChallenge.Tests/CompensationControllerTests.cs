using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeChallenge.Models;
using CodeChallenge.Tests.Integration.Constants;
using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeChallenge.Tests.Integration;

[TestClass]
public class CompensationControllerTests
{
    private static HttpClient _httpClient;
    private static TestServer _testServer;

    [ClassInitialize]
    // Attribute ClassInitialize requires this signature
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void InitializeClass(TestContext context)
    {
        _testServer = new TestServer();
        _httpClient = _testServer.NewClient();
    }

    [ClassCleanup]
    public static void CleanUpTest()
    {
        _httpClient.Dispose();
        _testServer.Dispose();
    }

    [TestMethod]
    public async Task CreateCompensation_EmployeeExists_NoPreviousCompensation_Returns_Created()
    {
        // Arrange
        var compensation = new CompensationCreateRequest
        {
            EmployeeId = TestEmployeeIds.RingoStarr,
            Salary = 90000.00,
            EffectiveDate = new DateOnly(2023, 06, 15).ToString()
        };
        var expected = new Compensation
        {
            EffectiveDate = compensation.EffectiveDate,
            Salary = compensation.Salary,
            EmployeeId = compensation.EmployeeId,
            Employee = new Employee
            {
                EmployeeId = compensation.EmployeeId,
                FirstName = "Ringo",
                LastName = "Starr",
                Department = "Engineering",
                Position = "Developer V"
            }
        };
        var requestContent = new JsonSerialization().ToJson(compensation);

        // Act
        var response = await _httpClient.PostAsync("api/compensation",
            new StringContent(requestContent, Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.Created);
        var result = response.DeserializeContent<Compensation>();

        result.Should().BeEquivalentTo(expected, options => options
            .Excluding(x => x.CompensationId)
            .Excluding(x => x.Employee.DirectReports));

        result.EmployeeId.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CreateCompensation_EmployeeExists_HasPreviousCompensation_Returns_Created()
    {
        // Arrange
        var compensation = new CompensationCreateRequest
        {
            EmployeeId = TestEmployeeIds.PaulMcCartney,
            Salary = 110000.00,
            EffectiveDate = new DateOnly(2024, 01, 15).ToString()
        };
        var expected = new Compensation
        {
            EffectiveDate = compensation.EffectiveDate,
            Salary = compensation.Salary,
            EmployeeId = compensation.EmployeeId,
            Employee = new Employee
            {
                EmployeeId = compensation.EmployeeId,
                FirstName = "Paul",
                LastName = "McCartney",
                Department = "Engineering",
                Position = "Developer I"
            }
        };
        var requestContent = new JsonSerialization().ToJson(compensation);

        // Act
        var response = await _httpClient.PostAsync("api/compensation",
            new StringContent(requestContent, Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.Created);
        var result = response.DeserializeContent<Compensation>();

        result.Should().BeEquivalentTo(expected, options => options
            .Excluding(x => x.CompensationId)
            .Excluding(x => x.Employee.DirectReports));

        result.EmployeeId.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CreateCompensation_EmployeeDoesNotExist_Returns_NotFound()
    {
        // Arrange
        var compensation = new CompensationCreateRequest
        {
            EmployeeId = "Invalid_Id",
            Salary = 100000.00,
            EffectiveDate = new DateOnly(2023, 03, 15).ToString()
        };
        var requestContent = new JsonSerialization().ToJson(compensation);

        // Act
        var response = await _httpClient.PostAsync("api/compensation",
            new StringContent(requestContent, Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetByEmployeeId_EmployeeExists_HasCompensation_Ok()
    {
        // Arrange
        var employeeId = TestEmployeeIds.JohnLennon;
        var expected = new Compensation
        {
            EffectiveDate = new DateOnly(2023, 03, 15).ToString(),
            Salary = 100000.00,
            EmployeeId = employeeId,
            Employee = new Employee
            {
                EmployeeId = employeeId,
                FirstName = "John",
                LastName = "Lennon",
                Department = "Engineering",
                Position = "Development Manager"
            }
        };

        // Act
        var response = await _httpClient.GetAsync($"api/compensation/{employeeId}");

        // Assert
        response.Should().BeSuccessful();

        var result = response.DeserializeContent<Compensation>();

        result.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.CompensationId));
    }

    [TestMethod]
    public async Task GetByEmployeeId_EmployeeExists_DoesNotHaveCompensation_NotFound()
    {
        // Arrange
        var employeeId = TestEmployeeIds.PeteBest;

        // Act
        var response = await _httpClient.GetAsync($"api/compensation/{employeeId}");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetByEmployeeId_EmployeeDoesNotExist_NotFound()
    {
        // Arrange
        var employeeId = "Invalid_Id";

        // Act
        var response = await _httpClient.GetAsync($"api/compensation/{employeeId}");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }
}
