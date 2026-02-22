using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Provider;
using MyIndustry.Domain.ValueObjects;
using RabbitMqCommunicator;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;

namespace MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;

public sealed record CreateSellerCommandHandler : IRequestHandler<CreateSellerCommand, CreateSellerCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<DomainSellerSubscription> _sellerSubscriptionRepository;
    private readonly IGenericRepository<DomainSubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityProvider _securityProvider;

    public CreateSellerCommandHandler(
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IGenericRepository<DomainSellerSubscription> sellerSubscriptionRepository,
        IGenericRepository<DomainSubscriptionPlan> subscriptionPlanRepository,
        IUnitOfWork unitOfWork,
        ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _unitOfWork = unitOfWork;
        _securityProvider = securityProvider;
    }

    public async Task<CreateSellerCommandResult> Handle(CreateSellerCommand request,
        CancellationToken cancellationToken)
    {
        string encryptedIdentityNumber = null;
        if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
        {
            encryptedIdentityNumber = _securityProvider.EncryptAes256(request.IdentityNumber);
        }

        var existingSeller = await _sellerRepository
            .GetAllQuery()
            .Include(s => s.SellerSubscriptions)
            .FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);

        if (existingSeller != null)
        {
            var hasActiveSubscription = existingSeller.SellerSubscriptions?.Any(s => s.IsActive) == true;
            if (!hasActiveSubscription)
            {
                var freePlanForExisting = await _subscriptionPlanRepository
                    .GetAllQuery()
                    .FirstOrDefaultAsync(p => p.SubscriptionType == SubscriptionType.Free && p.IsActive, cancellationToken);

                if (freePlanForExisting != null)
                {
                    await _sellerSubscriptionRepository.AddAsync(new DomainSellerSubscription
                    {
                        Id = Guid.NewGuid(),
                        SellerId = existingSeller.Id,
                        SubscriptionPlanId = freePlanForExisting.Id,
                        StartDate = DateTime.UtcNow,
                        ExpiryDate = DateTime.UtcNow.AddDays(freePlanForExisting.PostDurationInDays),
                        RemainingPostQuota = freePlanForExisting.MonthlyPostLimit,
                        RemainingFeaturedQuota = freePlanForExisting.FeaturedPostLimit,
                        IsAutoRenew = true,
                        IsActive = true
                    }, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            return new CreateSellerCommandResult().ReturnOk();
        }

        // Check if identity number is already used by another seller
        var identityExists =
            await _sellerRepository.AnyAsync(p => p.IdentityNumber == encryptedIdentityNumber && p.Id != request.UserId, cancellationToken);

        if (identityExists)
            throw new BusinessRuleException("Aynı VKN/TCKN ile başka bir satıcı mevcut.");

        // Get the free subscription plan
        var freePlan = await _subscriptionPlanRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(p => p.SubscriptionType == SubscriptionType.Free && p.IsActive, cancellationToken);

        if (freePlan == null)
            throw new BusinessRuleException("Ücretsiz abonelik planı bulunamadı. Lütfen yönetici ile iletişime geçin.");

        var seller = new Domain.Aggregate.Seller
        {
            Id = request.UserId,
            Description = request.Description,
            Sector = request.Sector,
            Title = request.Title,
            IdentityNumber = encryptedIdentityNumber,
            AgreementUrl = request.AgreementUrl,
            IsActive = true,
            SellerInfo = new SellerInfo
            {
                Id = Guid.NewGuid(),
                SellerId = request.UserId,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            }
        };

        await _sellerRepository.AddAsync(seller, cancellationToken);

        await _sellerSubscriptionRepository.AddAsync(new DomainSellerSubscription
        {
            Id = Guid.NewGuid(),
            SellerId = request.UserId,
            SubscriptionPlanId = freePlan.Id,
            StartDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(freePlan.PostDurationInDays),
            RemainingPostQuota = freePlan.MonthlyPostLimit,
            RemainingFeaturedQuota = freePlan.FeaturedPostLimit,
            IsAutoRenew = true,
            IsActive = true
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSellerCommandResult().ReturnOk();
    }
}