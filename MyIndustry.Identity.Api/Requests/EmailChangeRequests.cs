namespace MyIndustry.Identity.Api.Requests;

public record SendEmailChangeVerificationRequest(string NewEmail);
public record VerifyEmailChangeRequest(string Code);
