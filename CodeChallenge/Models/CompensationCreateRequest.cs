namespace CodeChallenge.Models;

public class CompensationCreateRequest
{
    public string EmployeeId { get; set; }
    public double Salary { get; set; }
    public string EffectiveDate { get; set; }
}
