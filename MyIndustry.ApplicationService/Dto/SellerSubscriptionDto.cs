namespace MyIndustry.ApplicationService.Dto;

public class SellerSubscriptionDto
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime EndDate { get; set; }

    public int RemainingPostQuota { get; set; }
    public int RemainingFeaturedQuota { get; set; }

    public bool IsAutoRenew { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string Name { get; set; }
    public string SubscriptionPlanName { get; set; }
}