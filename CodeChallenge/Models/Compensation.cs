namespace CodeChallenge.Models;

public class Compensation
{
    public string CompensationId { get; set; }
    public double Salary { get; set; }
    public string EffectiveDate { get; set; }
    public string EmployeeId { get; set; }
    public Employee Employee { get; set; }
}
