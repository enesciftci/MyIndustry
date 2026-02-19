using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainPurchaser = MyIndustry.Domain.Aggregate.Purchaser;
using DomainService = MyIndustry.Domain.Aggregate.Service;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.ApplicationService.Handler.Admin.GetAdminStatsQuery;

public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, GetAdminStatsQueryResult>
{
    private readonly IGenericRepository<DomainSeller> _sellerRepository;
    private readonly IGenericRepository<DomainPurchaser> _purchaserRepository;
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IGenericRepository<DomainCategory> _categoryRepository;

    public GetAdminStatsQueryHandler(
        IGenericRepository<DomainSeller> sellerRepository,
        IGenericRepository<DomainPurchaser> purchaserRepository,
        IGenericRepository<DomainService> serviceRepository,
        IGenericRepository<DomainMessage> messageRepository,
        IGenericRepository<DomainCategory> categoryRepository)
    {
        _sellerRepository = sellerRepository;
        _purchaserRepository = purchaserRepository;
        _serviceRepository = serviceRepository;
        _messageRepository = messageRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<GetAdminStatsQueryResult> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var sixMonthsAgo = now.AddMonths(-6);

        // User Stats
        var totalSellers = await _sellerRepository.GetAllQuery().CountAsync(cancellationToken);
        var totalBuyers = await _purchaserRepository.GetAllQuery().CountAsync(cancellationToken);
        var newSellersThisMonth = await _sellerRepository.GetAllQuery()
            .CountAsync(s => s.CreatedDate >= startOfMonth, cancellationToken);
        var newBuyersThisMonth = await _purchaserRepository.GetAllQuery()
            .CountAsync(p => p.CreatedDate >= startOfMonth, cancellationToken);

        // Listing Stats
        var allListings = await _serviceRepository.GetAllQuery().ToListAsync(cancellationToken);
        var totalListings = allListings.Count;
        var pendingListings = allListings.Count(s => !s.IsApproved && s.IsActive);
        var approvedListings = allListings.Count(s => s.IsApproved && s.IsActive);
        var rejectedListings = allListings.Count(s => !s.IsActive);
        var newListingsThisMonth = allListings.Count(s => s.CreatedDate >= startOfMonth);

        // Message Stats
        var totalMessages = await _messageRepository.GetAllQuery().CountAsync(cancellationToken);
        var messagesThisMonth = await _messageRepository.GetAllQuery()
            .CountAsync(m => m.CreatedDate >= startOfMonth, cancellationToken);

        // Category Stats
        var totalCategories = await _categoryRepository.GetAllQuery().CountAsync(cancellationToken);

        // Recent Activities
        var recentListings = await _serviceRepository.GetAllQuery()
            .OrderByDescending(s => s.CreatedDate)
            .Take(5)
            .Select(s => new RecentActivityDto
            {
                Type = "listing_created",
                Description = $"Yeni ilan: {s.Title}",
                Date = s.CreatedDate,
                EntityId = s.Id
            })
            .ToListAsync(cancellationToken);

        var recentSellers = await _sellerRepository.GetAllQuery()
            .OrderByDescending(s => s.CreatedDate)
            .Take(5)
            .Select(s => new RecentActivityDto
            {
                Type = "seller_registered",
                Description = $"Yeni satıcı: {s.Title}",
                Date = s.CreatedDate,
                EntityId = s.Id
            })
            .ToListAsync(cancellationToken);

        var recentActivities = recentListings
            .Concat(recentSellers)
            .OrderByDescending(a => a.Date)
            .Take(10)
            .ToList();

        // Chart Data - Last 6 months
        var userChart = new List<ChartDataPoint>();
        var listingChart = new List<ChartDataPoint>();

        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var monthLabel = monthStart.ToString("MMM yyyy");

            var sellersInMonth = await _sellerRepository.GetAllQuery()
                .CountAsync(s => s.CreatedDate >= monthStart && s.CreatedDate < monthEnd, cancellationToken);
            var buyersInMonth = await _purchaserRepository.GetAllQuery()
                .CountAsync(p => p.CreatedDate >= monthStart && p.CreatedDate < monthEnd, cancellationToken);
            
            userChart.Add(new ChartDataPoint { Label = monthLabel, Value = sellersInMonth + buyersInMonth });

            var listingsInMonth = allListings.Count(s => s.CreatedDate >= monthStart && s.CreatedDate < monthEnd);
            listingChart.Add(new ChartDataPoint { Label = monthLabel, Value = listingsInMonth });
        }

        var stats = new AdminStatsDto
        {
            TotalUsers = totalSellers + totalBuyers,
            TotalSellers = totalSellers,
            TotalBuyers = totalBuyers,
            NewUsersThisMonth = newSellersThisMonth + newBuyersThisMonth,
            TotalListings = totalListings,
            PendingListings = pendingListings,
            ApprovedListings = approvedListings,
            RejectedListings = rejectedListings,
            NewListingsThisMonth = newListingsThisMonth,
            TotalMessages = totalMessages,
            MessagesThisMonth = messagesThisMonth,
            TotalCategories = totalCategories,
            RecentActivities = recentActivities,
            UserRegistrationChart = userChart,
            ListingChart = listingChart
        };

        return new GetAdminStatsQueryResult { Stats = stats }.ReturnOk();
    }
}
