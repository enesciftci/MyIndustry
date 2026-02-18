namespace MyIndustry.ApplicationService.Handler.Verification.SendEmailChangeVerificationCommand;

public record SendEmailChangeVerificationCommandResult : ResponseBase
{
    public int ExpiresInSeconds { get; set; }
}
