namespace MyIndustry.Domain.Aggregate;

public class PhoneVerification : Entity
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; }
    public string VerificationCode { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
}
