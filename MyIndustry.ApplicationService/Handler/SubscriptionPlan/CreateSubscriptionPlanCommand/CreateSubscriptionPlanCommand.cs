using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.CreateSubscriptionPlanCommand;

public record CreateSubscriptionPlanCommand : IRequest<CreateSubscriptionPlanCommandResult>
{
    public string Description { get; set; }
    public string Name { get; set; }
    public decimal MonthlyPrice { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public int FeaturedPostLimit { get; set; }
    public int MonthlyPostLimit { get; set; }
    public int PostDurationInDays { get; set; }
}