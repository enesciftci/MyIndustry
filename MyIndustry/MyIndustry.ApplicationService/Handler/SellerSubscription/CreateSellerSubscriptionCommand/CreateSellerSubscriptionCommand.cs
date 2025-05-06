namespace MyIndustry.ApplicationService.Handler.SellerSubscription.CreateSellerSubscriptionCommand;

public record CreateSellerSubscriptionCommand : IRequest<CreateSellerSubscriptionCommandResult>
{
    public Guid SellerId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public bool IsAutoRenew { get; set; }
}