namespace MyIndustry.Identity.Domain.Service;

public class AuthenticationModel
{
    public string Message { get; set; }
    public bool IsAuthenticated { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public UserDto User { get; set; }
}