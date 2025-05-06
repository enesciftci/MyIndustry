namespace MyIndustry.ApplicationService.Dto;

public class SellerSubscriptionDto
{
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public int RemainingPostQuota { get; set; }
    public int RemainingFeaturedQuota { get; set; }

    public bool IsAutoRenew { get; set; } // ✅ Yeni eklenen alan
    public Guid SubscriptionPlanId { get; set; }
    public string Name { get; set; }
}