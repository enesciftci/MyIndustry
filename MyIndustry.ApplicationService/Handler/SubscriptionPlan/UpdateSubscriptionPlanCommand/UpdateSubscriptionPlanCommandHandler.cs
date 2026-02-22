using MyIndustry.Domain.ExceptionHandling;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.UpdateSubscriptionPlanCommand;

public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, UpdateSubscriptionPlanCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSubscriptionPlanCommandHandler(
        IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository, 
        IUnitOfWork unitOfWork)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateSubscriptionPlanCommandResult> Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _subscriptionPlanRepository.GetById(request.Id, cancellationToken);
        
        if (plan == null)
        {
            throw new BusinessRuleException("Abonelik planı bulunamadı.");
        }

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.SubscriptionType = request.SubscriptionType;
        plan.FeaturedPostLimit = request.FeaturedPostLimit;
        plan.MonthlyPostLimit = request.MonthlyPostLimit;
        plan.PostDurationInDays = request.PostDurationInDays;
        plan.IsActive = request.IsActive;
        plan.ModifiedDate = DateTime.UtcNow;

        _subscriptionPlanRepository.Update(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateSubscriptionPlanCommandResult().ReturnOk();
    }
}
