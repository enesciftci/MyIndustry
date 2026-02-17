namespace MyIndustry.Identity.Domain.Aggregate;

public class EmailVerificationCode
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    
    public virtual ApplicationUser User { get; set; }
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;
}
