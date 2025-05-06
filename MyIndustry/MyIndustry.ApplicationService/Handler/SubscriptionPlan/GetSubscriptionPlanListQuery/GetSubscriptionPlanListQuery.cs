namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetSubscriptionPlanListQuery;

public sealed record GetSubscriptionPlanListQuery : IRequest<GetSubscriptionPlanListQueryResult>
{
    public Guid SellerId { get; set; }
}