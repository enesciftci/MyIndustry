namespace MyIndustry.Queue.Message;

public record SendPhoneVerificationMessage
{
    public string PhoneNumber { get; set; }
    public string VerificationCode { get; set; }
}
