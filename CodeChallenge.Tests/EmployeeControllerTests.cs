using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeChallenge.Models;
using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeChallenge.Tests.Integration;

[TestClass]
public class EmployeeControllerTests
{
    private static HttpClient _httpClient;
    private static TestServer _testServer;

    [ClassInitialize]
    // Attribute ClassInitialize requires this signature
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
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
    public async Task CreateEmployee_Returns_Created()
    {
        // Arrange
        var employee = new Employee
        {
            Department = "Complaints",
            FirstName = "Debbie",
            LastName = "Downer",
            Position = "Receiver",
        };

        var requestContent = new JsonSerialization().ToJson(employee);

        // Act
        var response = await _httpClient.PostAsync("api/employee",
            new StringContent(requestContent, Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.Created);
        var newEmployee = response.DeserializeContent<Employee>();

        newEmployee.Should().BeEquivalentTo(employee, options => options.Excluding(x => x.EmployeeId));
        newEmployee.EmployeeId.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GetEmployeeById_Returns_Ok()
    {
        // Arrange
        var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
        var expected = new Employee
        {
            EmployeeId = employeeId,
            FirstName = "John",
            LastName = "Lennon",
            Department = "Engineering",
            Position = "Development Manager"
        };

        // Act
        var response = await _httpClient.GetAsync($"api/employee/{employeeId}");

        // Assert
        response.Should().BeSuccessful();

        var employee = response.DeserializeContent<Employee>();

        // Ignores Direct Reports
        employee.Should().BeEquivalentTo(expected, options => options.ExcludingMissingMembers());
    }

    [TestMethod]
    public async Task UpdateEmployee_Returns_Ok()
    {
        // Arrange
        var update = new Employee
        {
            EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
            Department = "Engineering",
            FirstName = "Pete",
            LastName = "Best",
            Position = "Developer VI",
        };
        var expected = new Employee
        {
            EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
            Department = "Engineering",
            FirstName = "Pete",
            LastName = "Best",
            Position = "Developer VI"
        };

        var requestContent = new JsonSerialization().ToJson(update);

        // Execute
        var putResponse = await _httpClient.PutAsync($"api/employee/{update.EmployeeId}",
            new StringContent(requestContent, Encoding.UTF8, "application/json"));

        // Assert successful put request
        putResponse.Should().BeSuccessful();
        var newEmployee = putResponse.DeserializeContent<Employee>();
        newEmployee.Should().BeEquivalentTo(expected, options => options.ExcludingMissingMembers());

        // Assert record was updated using get request
        var getResponse = await _httpClient.GetAsync($"api/employee/{update.EmployeeId}");
        getResponse.Should().BeSuccessful();
        var result = putResponse.DeserializeContent<Employee>();
        result.Should().BeEquivalentTo(expected, options => options.ExcludingMissingMembers());
    }

    [TestMethod]
    public async Task UpdateEmployee_Returns_NotFound()
    {
        // Arrange
        var employee = new Employee
        {
            EmployeeId = "Invalid_Id",
            Department = "Music",
            FirstName = "Sunny",
            LastName = "Bono",
            Position = "Singer/Song Writer",
        };
        var requestContent = new JsonSerialization().ToJson(employee);

        // Execute
        var response = await _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
            new StringContent(requestContent, Encoding.UTF8, "application/json"));

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }


    [TestMethod]
    public async Task GetReportingStructureByEmployeeId_HasSubReports_Ok()
    {
        // Arrange
        var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
        var expected = new ReportingStructure
        {
            Employee = new Employee
            {
                EmployeeId = employeeId,
                FirstName = "John",
                LastName = "Lennon",
                Department = "Engineering",
                Position = "Development Manager"
            },
            NumberOfReports = 4
        };

        // Act
        var response = await _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");

        // Assert
        response.Should().BeSuccessful();

        var result = response.DeserializeContent<ReportingStructure>();

        result.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.Employee.DirectReports));
    }

    [TestMethod]
    public async Task GetReportingStructureByEmployeeId_NoSubReports_Ok()
    {
        // Arrange
        var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";
        var expected = new ReportingStructure
        {
            Employee = new Employee
            {
                EmployeeId = employeeId,
                FirstName = "Ringo",
                LastName = "Starr",
                Department = "Engineering",
                Position = "Developer V"
            },
            NumberOfReports = 2
        };

        // Act
        var response = await _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");

        // Assert
        response.Should().BeSuccessful();

        var result = response.DeserializeContent<ReportingStructure>();

        result.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.Employee.DirectReports));
    }

    [TestMethod]
    public async Task GetReportingStructureByEmployeeId_NoReports_Ok()
    {
        // Arrange
        var employeeId = "b7839309-3348-463b-a7e3-5de1c168beb3";
        var expected = new ReportingStructure
        {
            Employee = new Employee
            {
                EmployeeId = employeeId,
                FirstName = "Paul",
                LastName = "McCartney",
                Department = "Engineering",
                Position = "Developer I"
            },
            NumberOfReports = 0
        };

        // Act
        var response = await _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");

        // Assert
        response.Should().BeSuccessful();

        var result = response.DeserializeContent<ReportingStructure>();

        result.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.Employee.DirectReports));
    }

    [TestMethod]
    public async Task GetReportingStructureByEmployeeId_EmployeeDoesNotExist_NotFound()
    {
        // Arrange
        var employeeId = "a8e853c6-0286-41a4-9955-9bd9d8afb189";

        // Act
        var response = await _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }
}