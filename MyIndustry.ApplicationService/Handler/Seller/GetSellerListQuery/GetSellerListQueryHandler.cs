using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.Provider;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;

public sealed class GetSellerListQueryHandler : IRequestHandler<GetSellerListQuery,GetSellerListQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly ISecurityProvider _securityProvider;

    public GetSellerListQueryHandler(IGenericRepository<Domain.Aggregate.Seller> sellerRepository, ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _securityProvider = securityProvider;
    }

    public async Task<GetSellerListQueryResult> Handle(GetSellerListQuery request, CancellationToken cancellationToken)
    {
        var sellers = await _sellerRepository
            .GetAllQuery()
            .Include(s => s.Services)
            .Include(s => s.SellerInfo)
            .Skip((request.Pager.Index - 1) * request.Pager.Size)
            .Take(request.Pager.Size)
            .Select(p => new SellerDto()
            {
                Id = p.Id,
                CreatedDate = p.CreatedDate,
                Description = p.Description,
                Sector = p.Sector,
                Title = p.Title,
                AgreementUrl = p.AgreementUrl,
                ServiceCount = p.Services.Count(s => s.IsActive && s.IsApproved),
                IsVerified = p.IsActive,
                Logo = p.SellerInfo != null ? p.SellerInfo.LogoUrl : null,
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
            })
            .ToListAsync(cancellationToken);

        return new GetSellerListQueryResult()
        {
            Sellers = sellers
        }.ReturnOk();

    }
}