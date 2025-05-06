namespace MyIndustry.ApplicationService.Handler.SellerSubscription.GetSellerSubscriptionQuery;

public record GetSellerSubscriptionQuery : IRequest<GetSellerSubscriptionQueryResult>
{
    public Guid SellerId { get; set; }
}