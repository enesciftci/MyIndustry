using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.ExceptionHandling;

namespace MyIndustry.ApplicationService.Handler.SellerSubscription.UpgradeSellerSubscriptionCommand;

public class UpgradeSellerSubscriptionCommandHandler : IRequestHandler<UpgradeSellerSubscriptionCommand, UpgradeSellerSubscriptionCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _sellerSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeSellerSubscriptionCommandHandler(
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository,
        IGenericRepository<Domain.Aggregate.SellerSubscription> sellerSubscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _sellerRepository = sellerRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpgradeSellerSubscriptionCommandResult> Handle(UpgradeSellerSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository
            .GetAllQuery()
            .Where(p => p.Id == request.SellerId)
            .Include(p => p.SellerSubscription)
            .FirstOrDefaultAsync(cancellationToken);

        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı.");

        var subscriptionPlan = await _subscriptionPlanRepository
            .GetById(p => p.Id == request.SubscriptionPlanId && p.IsActive, cancellationToken);

        if (subscriptionPlan == null)
            throw new BusinessRuleException("Abonelik planı bulunamadı veya aktif değil.");

        // Mevcut aktif aboneliği kontrol et
        var currentSubscription = await _sellerSubscriptionRepository
            .GetAllQuery()
            .Where(s => s.SellerId == request.SellerId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentSubscription == null)
            throw new BusinessRuleException("Aktif bir aboneliğiniz bulunmamaktadır. Lütfen önce abonelik oluşturun.");

        // Aynı plana yükseltme yapılamaz
        if (currentSubscription.SubscriptionPlanId == request.SubscriptionPlanId)
            throw new BusinessRuleException("Zaten bu plana sahipsiniz.");

        // Mevcut aboneliği pasif yap
        currentSubscription.IsActive = false;
        _sellerSubscriptionRepository.Update(currentSubscription);

        // Yeni abonelik oluştur
        var newSubscription = new Domain.Aggregate.SellerSubscription
        {
            SellerId = request.SellerId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            IsAutoRenew = request.IsAutoRenew,
            StartDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            RemainingFeaturedQuota = subscriptionPlan.FeaturedPostLimit,
            RemainingPostQuota = subscriptionPlan.MonthlyPostLimit,
            IsActive = true
        };

        await _sellerSubscriptionRepository.AddAsync(newSubscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpgradeSellerSubscriptionCommandResult().ReturnOk("Paket başarıyla yükseltildi.");
    }
}
