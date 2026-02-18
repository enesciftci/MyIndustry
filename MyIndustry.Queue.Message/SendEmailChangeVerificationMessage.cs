namespace MyIndustry.Queue.Message;

public record SendEmailChangeVerificationMessage
{
    public string Email { get; set; }
    public string VerificationCode { get; set; }
}
