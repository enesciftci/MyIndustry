using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.SellerSubscription.GetSellerSubscriptionQuery;

public record GetSellerSubscriptionQueryResult : ResponseBase
{
    public SellerSubscriptionDto SellerSubscriptionDto { get; set; }
}