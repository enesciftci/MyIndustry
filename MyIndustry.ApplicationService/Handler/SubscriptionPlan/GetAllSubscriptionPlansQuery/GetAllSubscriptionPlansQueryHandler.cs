using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetAllSubscriptionPlansQuery;

public class GetAllSubscriptionPlansQueryHandler : IRequestHandler<GetAllSubscriptionPlansQuery, GetAllSubscriptionPlansQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.SubscriptionPlan> _subscriptionPlanRepository;

    public GetAllSubscriptionPlansQueryHandler(IGenericRepository<Domain.Aggregate.SubscriptionPlan> subscriptionPlanRepository)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
    }

    public async Task<GetAllSubscriptionPlansQueryResult> Handle(GetAllSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _subscriptionPlanRepository
            .GetAllQuery()
            .OrderBy(p => p.SubscriptionType)
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SubscriptionType = p.SubscriptionType.ToString(),
                MonthlyPrice = p.MonthlyPrice,
                MonthlyPostLimit = p.MonthlyPostLimit,
                PostDurationInDays = p.PostDurationInDays,
                FeaturedPostLimit = p.FeaturedPostLimit,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        return new GetAllSubscriptionPlansQueryResult() { Plans = plans }.ReturnOk();
    }
}
