namespace MyIndustry.Identity.Api.Requests;

public class PasswordResetRequest
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}