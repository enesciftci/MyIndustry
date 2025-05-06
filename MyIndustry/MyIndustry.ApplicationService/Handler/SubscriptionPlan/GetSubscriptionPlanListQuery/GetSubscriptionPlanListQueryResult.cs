using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetSubscriptionPlanListQuery;

public sealed record GetSubscriptionPlanListQueryResult : ResponseBase
{
    public List<SubscriptionPlanDto> SubscriptionPlanList { get; set; }
}