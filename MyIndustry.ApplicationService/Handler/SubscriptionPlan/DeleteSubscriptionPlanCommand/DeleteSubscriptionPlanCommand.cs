namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.DeleteSubscriptionPlanCommand;

public record DeleteSubscriptionPlanCommand : IRequest<DeleteSubscriptionPlanCommandResult>
{
    public Guid Id { get; set; }
}
