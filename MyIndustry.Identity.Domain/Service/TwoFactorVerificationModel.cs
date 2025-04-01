namespace MyIndustry.Identity.Domain.Service;

public class TwoFactorVerificationModel
{
    public string Email { get; set; }
    public string VerificationCode { get; set; }
}