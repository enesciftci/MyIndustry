using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.UpdateSubscriptionPlanCommand;

public record UpdateSubscriptionPlanCommand : IRequest<UpdateSubscriptionPlanCommandResult>
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public decimal MonthlyPrice { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public int FeaturedPostLimit { get; set; }
    public int MonthlyPostLimit { get; set; }
    public int PostDurationInDays { get; set; }
    public bool IsActive { get; set; }
}
