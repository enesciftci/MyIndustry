using Microsoft.EntityFrameworkCore;

namespace MyIndustry.ApplicationService.Handler.SellerSubscription.CreateSellerSubscriptionCommand;

public class CreateSellerSubscriptionCommandHandler : IRequestHandler<CreateSellerSubscriptionCommand, CreateSellerSubscriptionCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _sellerSubscriptionRepository;
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSellerSubscriptionCommandHandler(
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IUnitOfWork unitOfWork,
        IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository,
        IGenericRepository<Domain.Aggregate.SellerSubscription> sellerSubscriptionRepository)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
    }

    public async Task<CreateSellerSubscriptionCommandResult> Handle(CreateSellerSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository
            .GetAllQuery()
            .Where(p => p.Id == request.SellerId)
            .Include(p => p.SellerSubscriptions)
            .FirstOrDefaultAsync(cancellationToken);

        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı.");
        if (seller.SellerSubscriptions?.Any(s => s.IsActive) == true)
            throw new BusinessRuleException("Satıcı aboneliği bulunuyor.");

        var subscriptionPlan = await _subscriptionPlanRepository
            .GetById(p => p.Id == request.SubscriptionPlanId && p.IsActive, cancellationToken);

        if (subscriptionPlan == null)
            throw new BusinessRuleException("Abonelik planı bulunamadı.");

        var sellerSubscription = new Domain.Aggregate.SellerSubscription
        {
            SellerId = request.SellerId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            IsAutoRenew = request.IsAutoRenew,
            ExpiryDate = DateTime.UtcNow.AddDays(subscriptionPlan.PostDurationInDays),
            StartDate = DateTime.UtcNow,
            RemainingFeaturedQuota = subscriptionPlan.FeaturedPostLimit,
            RemainingPostQuota = subscriptionPlan.MonthlyPostLimit,
            IsActive = true
        };

        await _sellerSubscriptionRepository.AddAsync(sellerSubscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSellerSubscriptionCommandResult().ReturnOk();
    }
}