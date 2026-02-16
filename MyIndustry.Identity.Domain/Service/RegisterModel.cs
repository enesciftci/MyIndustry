using MyIndustry.Identity.Domain.Aggregate.ValueObjects;

namespace MyIndustry.Identity.Domain.Service;

public class RegisterModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ConfirmPassword { get; set; }
    public UserType UserType { get; set; }
}