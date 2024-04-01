using System.Collections.Generic;

namespace CodeChallenge.Models;

public class ReportingStructure
{
    public Employee Employee { get; set; }
    public IEnumerable<Employee> Reports { get; set; }
    public int NumberOfReports { get; set; }
}
