namespace MyIndustry.ApplicationService.Handler.SellerSubscription.UpgradeSellerSubscriptionCommand;

public record UpgradeSellerSubscriptionCommand : IRequest<UpgradeSellerSubscriptionCommandResult>
{
    public Guid SellerId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public bool IsAutoRenew { get; set; }
}
