using MyIndustry.Identity.Domain.Aggregate.ValueObjects;

namespace MyIndustry.Identity.Domain.Service;

public class UserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public string Id { get; set; }
    public UserType UserType { get; set; }
}