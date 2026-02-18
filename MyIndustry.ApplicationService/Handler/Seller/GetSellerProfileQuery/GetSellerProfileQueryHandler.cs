using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerProfileQuery;

public sealed class GetSellerProfileQueryHandler : IRequestHandler<GetSellerProfileQuery, GetSellerProfileQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Favorite> _favoriteRepository;

    public GetSellerProfileQueryHandler(
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IGenericRepository<Domain.Aggregate.Service> serviceRepository,
        IGenericRepository<Domain.Aggregate.Favorite> favoriteRepository)
    {
        _sellerRepository = sellerRepository;
        _serviceRepository = serviceRepository;
        _favoriteRepository = favoriteRepository;
    }

    public async Task<GetSellerProfileQueryResult> Handle(GetSellerProfileQuery request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository
            .GetAllQuery()
            .Include(p => p.SellerInfo)
            .Include(p => p.SellerSubscription)
                .ThenInclude(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);

        if (seller == null)
        {
            return new GetSellerProfileQueryResult
            {
                Success = false,
                Message = "Satıcı bulunamadı"
            };
        }

        // Get service statistics
        var services = await _serviceRepository
            .GetAllQuery()
            .Where(s => s.SellerId == request.UserId)
            .ToListAsync(cancellationToken);

        var totalServices = services.Count;
        var activeServices = services.Count(s => s.IsActive && s.IsApproved);
        var totalViews = services.Sum(s => s.ViewCount);

        // Get total favorites for seller's services
        var serviceIds = services.Select(s => s.Id).ToList();
        var totalFavorites = await _favoriteRepository
            .GetAllQuery()
            .CountAsync(f => serviceIds.Contains(f.ServiceId), cancellationToken);

        var profileDto = new SellerProfileDto
        {
            Id = seller.Id,
            Title = seller.Title,
            Description = seller.Description,
            IdentityNumber = seller.IdentityNumber,
            Sector = (int)seller.Sector,
            IsActive = seller.IsActive,
            CreatedDate = seller.CreatedDate,
            
            // SellerInfo
            LogoUrl = seller.SellerInfo?.LogoUrl,
            PhoneNumber = seller.SellerInfo?.PhoneNumber,
            Email = seller.SellerInfo?.Email,
            TwitterUrl = seller.SellerInfo?.TwitterUrl,
            FacebookUrl = seller.SellerInfo?.FacebookUrl,
            InstagramUrl = seller.SellerInfo?.InstagramUrl,
            WebSiteUrl = seller.SellerInfo?.WebSiteUrl,
            
            // Statistics
            TotalServices = totalServices,
            ActiveServices = activeServices,
            TotalViews = totalViews,
            TotalFavorites = totalFavorites,
            
            // Subscription
            Subscription = seller.SellerSubscription != null ? new SellerSubscriptionDto
            {
                SubscriptionPlanId = seller.SellerSubscription.SubscriptionPlanId,
                StartDate = seller.SellerSubscription.StartDate,
                ExpiryDate = seller.SellerSubscription.ExpiryDate,
                Name = seller.SellerSubscription.SubscriptionPlan?.Name,
                RemainingPostQuota = seller.SellerSubscription.RemainingPostQuota,
                RemainingFeaturedQuota = seller.SellerSubscription.RemainingFeaturedQuota,
                IsAutoRenew = seller.SellerSubscription.IsAutoRenew
            } : null
        };

        return new GetSellerProfileQueryResult
        {
            Seller = profileDto
        }.ReturnOk();
    }
}
