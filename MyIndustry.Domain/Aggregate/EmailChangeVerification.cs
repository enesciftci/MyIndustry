namespace MyIndustry.Domain.Aggregate;

public class EmailChangeVerification : Entity
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; }
    public string VerificationCode { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
}
