using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetSubscriptionPlanListQuery;

public class GetSubscriptionPlanListQueryHandler : IRequestHandler<GetSubscriptionPlanListQuery, GetSubscriptionPlanListQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _sellerSubscriptionRepository;

    public GetSubscriptionPlanListQueryHandler(IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository, IGenericRepository<Domain.Aggregate.SellerSubscription> sellerSubscriptionRepository)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
    }

    public async Task<GetSubscriptionPlanListQueryResult> Handle(GetSubscriptionPlanListQuery request, CancellationToken cancellationToken)
    {
        var currentSubscription = await _sellerSubscriptionRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(p => p.SellerId == request.SellerId, cancellationToken);

        var currentPlanId = currentSubscription?.SubscriptionPlanId;

        var subscriptionPlansQuery = _subscriptionPlanRepository
            .GetAllQuery()
            .Where(p => p.IsActive);

        if (currentPlanId != null)
        {
            subscriptionPlansQuery = subscriptionPlansQuery
                .Where(p =>
                    p.Id != currentPlanId &&
                    p.SubscriptionType > currentSubscription.SubscriptionPlan.SubscriptionType &&
                    p.IsActive);
        }

        var subscriptionPlans = await subscriptionPlansQuery
            .OrderBy(p => p.MonthlyPrice) // Fiyata göre sırala (Free -> Standard -> Premium -> Corporate)
            .ThenBy(p => (int)p.SubscriptionType) // Aynı fiyat varsa enum sırasına göre
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SubscriptionType = p.SubscriptionType.ToString(),
                MonthlyPrice = p.MonthlyPrice,
                MonthlyPostLimit = p.MonthlyPostLimit,
                PostDurationInDays = p.PostDurationInDays,
                FeaturedPostLimit = p.FeaturedPostLimit
            })
            .ToListAsync(cancellationToken);

        return new GetSubscriptionPlanListQueryResult() { SubscriptionPlanList = subscriptionPlans }.ReturnOk();
    }
}