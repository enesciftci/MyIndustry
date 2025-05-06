namespace MyIndustry.Identity.Domain.Service;

public class UserDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public string Id { get; set; }
}