using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.ExceptionHandling;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.DeleteSubscriptionPlanCommand;

public class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand, DeleteSubscriptionPlanCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _sellerSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSubscriptionPlanCommandHandler(
        IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository,
        IGenericRepository<Domain.Aggregate.SellerSubscription> sellerSubscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteSubscriptionPlanCommandResult> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _subscriptionPlanRepository.GetById(request.Id, cancellationToken);
        
        if (plan == null)
        {
            throw new BusinessRuleException("Abonelik planı bulunamadı.");
        }

        // Check if plan is being used by any seller (active or inactive)
        var hasSubscriptions = await _sellerSubscriptionRepository
            .GetAllQuery()
            .AnyAsync(s => s.SubscriptionPlanId == request.Id, cancellationToken);

        if (hasSubscriptions)
        {
            throw new BusinessRuleException("Bu abonelik planına bağlı satıcı abonelikleri bulunmaktadır. Planı silemezsiniz.");
        }

        _subscriptionPlanRepository.Delete(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteSubscriptionPlanCommandResult().ReturnOk();
    }
}
