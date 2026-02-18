namespace MyIndustry.Identity.Api.Requests;

public class SendPhoneVerificationRequest
{
    public string PhoneNumber { get; set; } = default!;
}

public class VerifyPhoneRequest
{
    public string Code { get; set; } = default!;
}
