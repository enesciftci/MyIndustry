namespace MyIndustry.Domain.Aggregate;

public class SubscriptionCampaign : Entity
{
    public Guid SubscriptionPlanId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? FixedDiscountPrice { get; set; }
    public string? CouponCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }          // Max kaç defa kullanılabilir
    public int UsedCount { get; set; }            // Şu ana kadar kaç kez kullanıldı
    public bool IsOneTime { get; set; }           // Aynı kullanıcı sadece 1 kez mi kullanabilir
    public string? TargetAudienceTag { get; set; } // Örn: "new-users", "traders", "bulk-buyers"
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public SubscriptionPlan SubscriptionPlan { get; set; }
}
