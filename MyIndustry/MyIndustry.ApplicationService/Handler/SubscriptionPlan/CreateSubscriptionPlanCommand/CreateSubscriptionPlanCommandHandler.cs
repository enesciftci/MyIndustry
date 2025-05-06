namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.CreateSubscriptionPlanCommand;

public class CreateSubscriptionPlanCommandHandler :  IRequestHandler<CreateSubscriptionPlanCommand, CreateSubscriptionPlanCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubscriptionPlanCommandHandler(IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository, IUnitOfWork unitOfWork)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateSubscriptionPlanCommandResult> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        await _subscriptionPlanRepository.AddAsync(new Domain.Aggregate.SubscriptionPlan()
        {
            Description = request.Description,
            Name = request.Name,
            MonthlyPrice = request.MonthlyPrice,
            SubscriptionType = request.SubscriptionType,
            FeaturedPostLimit = request.FeaturedPostLimit,
            MonthlyPostLimit = request.MonthlyPostLimit,
            PostDurationInDays = request.PostDurationInDays,
            IsActive = true
        }, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSubscriptionPlanCommandResult().ReturnOk();
    }
    
}