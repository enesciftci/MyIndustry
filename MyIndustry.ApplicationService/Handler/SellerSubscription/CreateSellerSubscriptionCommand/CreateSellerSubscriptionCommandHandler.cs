using Microsoft.EntityFrameworkCore;

namespace MyIndustry.ApplicationService.Handler.SellerSubscription.CreateSellerSubscriptionCommand;

public class CreateSellerSubscriptionCommandHandler : IRequestHandler<CreateSellerSubscriptionCommand, CreateSellerSubscriptionCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSellerSubscriptionCommandHandler(IGenericRepository<Domain.Aggregate.Seller> sellerRepository, IUnitOfWork unitOfWork, IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _subscriptionPlanRepository = subscriptionPlanRepository;
    }

    public async Task<CreateSellerSubscriptionCommandResult> Handle(CreateSellerSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var seller =
            await _sellerRepository
                .GetAllQuery()
                .Where(p => p.Id == request.SellerId)
                .Include(p => p.SellerSubscription)
                .FirstOrDefaultAsync(cancellationToken);

        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı.");
        if(seller.SellerSubscription != null)
            throw new BusinessRuleException("Satıcı aboneliği bulunuyor.");

        var subscriptionPlan =
            await _subscriptionPlanRepository
                .GetById(p => p.Id == request.SubscriptionPlanId && p.IsActive, cancellationToken);

        if (subscriptionPlan == null)
            throw new BusinessRuleException("Abonelik planı bulunamadı.");
        
        var sellerSubscription = new Domain.Aggregate.SellerSubscription
        {
            SellerId = request.SellerId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            IsAutoRenew = request.IsAutoRenew,
            ExpiryDate = DateTime.Now.AddDays(30),
            StartDate = DateTime.Now,
            RemainingFeaturedQuota = subscriptionPlan.FeaturedPostLimit,
            RemainingPostQuota = subscriptionPlan.MonthlyPostLimit,
            SubscriptionPlan = subscriptionPlan,
            IsActive = true
        };
        
        seller.SellerSubscription = sellerSubscription;
        _sellerRepository.Update(seller);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSellerSubscriptionCommandResult().ReturnOk();
    }
}