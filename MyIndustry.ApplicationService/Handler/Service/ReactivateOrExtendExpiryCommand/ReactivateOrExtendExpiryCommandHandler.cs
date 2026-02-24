using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Handler.Service.ReactivateOrExtendExpiryCommand;

public class ReactivateOrExtendExpiryCommandHandler : IRequestHandler<ReactivateOrExtendExpiryCommand, ReactivateOrExtendExpiryCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _sellerSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateOrExtendExpiryCommandHandler(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository,
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IGenericRepository<Domain.Aggregate.SellerSubscription> sellerSubscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _sellerRepository = sellerRepository;
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReactivateOrExtendExpiryCommandResult> Handle(ReactivateOrExtendExpiryCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.SellerId == request.SellerId, cancellationToken);

        if (service == null)
            throw new BusinessRuleException("İlan bulunamadı.");

        var seller = await _sellerRepository
            .GetAllQuery()
            .Include(s => s.SellerSubscriptions!)
                .ThenInclude(ss => ss.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.Id == request.SellerId, cancellationToken);

        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı.");

        var activeSubscription = seller.SellerSubscriptions?.FirstOrDefault(ss => ss.IsActive);
        if (activeSubscription == null)
            throw new BusinessRuleException("Aktif abonelik bulunamadı. Lütfen abone olun.");

        var plan = activeSubscription.SubscriptionPlan;
        var postDurationDays = plan?.PostDurationInDays ?? 365;
        var newExpiryDate = DateTime.UtcNow.AddDays(postDurationDays);

        // Pasif ilanı tekrar aktif yapıyorsa kotadan düş
        if (!service.IsActive)
        {
            if (activeSubscription.RemainingPostQuota <= 0)
                throw new BusinessRuleException("Kalan ilan kotası dolu. Lütfen abonelik planınızı yükseltin.");
            activeSubscription.DecreaseRemainingPostQuota();
            _sellerSubscriptionRepository.Update(activeSubscription);
        }

        service.IsActive = true;
        service.ExpiryDate = newExpiryDate;
        _serviceRepository.Update(service);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ReactivateOrExtendExpiryCommandResult().ReturnOk();
    }
}
