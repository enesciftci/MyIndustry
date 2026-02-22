using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetAllSubscriptionPlansQuery;

public record GetAllSubscriptionPlansQueryResult : ResponseBase
{
    public List<SubscriptionPlanDto> Plans { get; set; }
}
