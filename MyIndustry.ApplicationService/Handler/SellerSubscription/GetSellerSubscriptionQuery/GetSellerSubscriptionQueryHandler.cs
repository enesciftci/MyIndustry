using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.SellerSubscription.GetSellerSubscriptionQuery;

public class GetSellerSubscriptionQueryHandler : IRequestHandler<GetSellerSubscriptionQuery, GetSellerSubscriptionQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _sellerSubscriptionRepository;

    public GetSellerSubscriptionQueryHandler(IGenericRepository<Domain.Aggregate.SellerSubscription> sellerSubscriptionRepository)
    {
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
    }


    public async Task<GetSellerSubscriptionQueryResult> Handle(GetSellerSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var sellerSubscription =
            await _sellerSubscriptionRepository
                .GetAllQuery()
                .Where(p => p.SellerId == request.SellerId && p.IsActive)
                .Include(p => p.SubscriptionPlan)
                .Select(x=>new SellerSubscriptionDto()
                {
                    Name = x.SubscriptionPlan.Name,
                    SubscriptionPlanId = x.SubscriptionPlanId,
                    StartDate = x.StartDate,
                    ExpiryDate = x.ExpiryDate,
                    IsAutoRenew = x.IsAutoRenew,
                    RemainingFeaturedQuota = x.RemainingFeaturedQuota,
                    RemainingPostQuota = x.RemainingPostQuota,
                })
                .FirstOrDefaultAsync(cancellationToken);
        
        if(sellerSubscription == null)
            throw new BusinessRuleException("Abonelik bulunamadı.");

        return new GetSellerSubscriptionQueryResult()
        {
            SellerSubscriptionDto = sellerSubscription,
        }.ReturnOk();
    }
}