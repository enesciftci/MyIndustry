namespace MyIndustry.Domain.Aggregate;

public class SellerSubscription : Entity
{
    public Guid SellerId { get; set; }
    public Guid SubscriptionPlanId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public int RemainingPostQuota { get; set; }
    public int RemainingFeaturedQuota { get; set; }

    public bool IsAutoRenew { get; set; } // ✅ Yeni eklenen alan

    public bool IsActive {get; set;}
    public Seller Seller { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; }

    public void DecreaseRemainingPostQuota()
    {
        RemainingPostQuota--;
    }
}