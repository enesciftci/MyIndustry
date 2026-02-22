using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.Provider;
using MyIndustry.Domain.ValueObjects;
using DomainCity = MyIndustry.Domain.Aggregate.City;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;

public sealed class GetSellerListQueryHandler : IRequestHandler<GetSellerListQuery,GetSellerListQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<DomainCity> _cityRepository;
    private readonly ISecurityProvider _securityProvider;

    public GetSellerListQueryHandler(
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository, 
        IGenericRepository<DomainCity> cityRepository,
        ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _cityRepository = cityRepository;
        _securityProvider = securityProvider;
    }

    public async Task<GetSellerListQueryResult> Handle(GetSellerListQuery request, CancellationToken cancellationToken)
    {
        // Get cities for location lookup
        var cities = await _cityRepository.GetAllQuery().ToListAsync(cancellationToken);
        
        var sellersData = await _sellerRepository
            .GetAllQuery()
            .Where(s => s.IsActive)
            .Include(s => s.Services)
            .Include(s => s.SellerInfo)
            .Include(s => s.Addresses)
            .OrderByDescending(s => s.CreatedDate)
            .Skip((request.Pager.Index - 1) * request.Pager.Size)
            .Take(request.Pager.Size)
            .ToListAsync(cancellationToken);

        var sellers = sellersData.Select(p => 
        {
            var mainAddress = p.Addresses?.FirstOrDefault(a => a.IsMain && a.IsActive) 
                           ?? p.Addresses?.FirstOrDefault(a => a.IsActive);
            var cityName = mainAddress != null 
                ? cities.FirstOrDefault(c => c.PlateCode == mainAddress.City)?.Name 
                : null;
            
            return new SellerDto
            {
                Id = p.Id,
                CreatedDate = p.CreatedDate,
                Description = p.Description,
                Sector = p.Sector,
                SectorName = GetEnumDescription(p.Sector),
                Title = p.Title,
                AgreementUrl = p.AgreementUrl,
                ServiceCount = p.Services?.Count(s => s.IsActive && s.IsApproved) ?? 0,
                IsVerified = p.IsActive,
                Logo = p.SellerInfo?.LogoUrl,
                Location = cityName,
                SellerInfo = p.SellerInfo != null ? new SellerInfoDto
                {
                    LogoUrl = p.SellerInfo.LogoUrl,
                    PhoneNumber = p.SellerInfo.PhoneNumber,
                    Email = p.SellerInfo.Email,
                    WebSiteUrl = p.SellerInfo.WebSiteUrl,
                    TwitterUrl = p.SellerInfo.TwitterUrl,
                    FacebookUrl = p.SellerInfo.FacebookUrl,
                    InstagramUrl = p.SellerInfo.InstagramUrl
                } : null
            };
        }).ToList();

        var totalCount = await _sellerRepository.GetAllQuery().Where(s => s.IsActive).CountAsync(cancellationToken);

        return new GetSellerListQueryResult()
        {
            Sellers = sellers,
            TotalCount = totalCount
        }.ReturnOk();
    }

    private static string GetEnumDescription(SellerSector sector)
    {
        var field = sector.GetType().GetField(sector.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? sector.ToString();
    }
}