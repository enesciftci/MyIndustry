namespace MyIndustry.ApplicationService.Handler.Verification.SendPhoneVerificationCommand;

public record SendPhoneVerificationCommandResult : ResponseBase
{
    public int ExpiresInSeconds { get; set; }
}
