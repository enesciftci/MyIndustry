using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.Domain.Aggregate;

public class SubscriptionPlan : Entity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int MonthlyPostLimit { get; set; }
    public int PostDurationInDays { get; set; }
    public int FeaturedPostLimit { get; set; }
    public bool IsActive { get; set; }
    public ICollection<SellerSubscription> SellerSubscriptions { get; set; }
    public ICollection<SubscriptionCampaign> SubscriptionCampaigns { get; set; }
}