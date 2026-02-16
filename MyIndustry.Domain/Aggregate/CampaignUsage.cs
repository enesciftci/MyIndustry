namespace MyIndustry.Domain.Aggregate;

public class CampaignUsage : Entity
{
    public Guid SellerId { get; set; }
    public Guid CampaignId { get; set; }
    public DateTime UsedAt { get; set; }
    public SubscriptionCampaign SubscriptionCampaign { get; set; }
    public Seller Seller { get; set; }
}