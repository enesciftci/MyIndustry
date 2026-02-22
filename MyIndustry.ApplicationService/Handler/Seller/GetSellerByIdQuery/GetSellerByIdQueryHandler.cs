using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerByIdQuery;

public class GetSellerByIdQueryHandler : IRequestHandler<GetSellerByIdQuery, GetSellerByIdQueryResult>
{
    private readonly IGenericRepository<DomainSeller> _sellerRepository;

    public GetSellerByIdQueryHandler(IGenericRepository<DomainSeller> sellerRepository)
    {
        _sellerRepository = sellerRepository;
    }

    public async Task<GetSellerByIdQueryResult> Handle(GetSellerByIdQuery request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository
            .GetAllQuery()
            .Include(s => s.SellerSubscriptions)
                .ThenInclude(ss => ss.SubscriptionPlan)
            .Include(s => s.SellerInfo)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == request.SellerId, cancellationToken);

        if (seller == null)
        {
            return new GetSellerByIdQueryResult().ReturnNotFound("Satıcı bulunamadı.");
        }

        var activeSub = seller.SellerSubscriptions?.FirstOrDefault(ss => ss.IsActive);

        var sellerDto = new SellerDto
        {
            Id = seller.Id,
            CreatedDate = seller.CreatedDate,
            Title = seller.Title,
            Description = seller.Description,
            Sector = seller.Sector,
            AgreementUrl = seller.AgreementUrl,
            IsVerified = seller.IsActive,
            ServiceCount = seller.Services?.Count(s => s.IsActive && s.IsApproved) ?? 0,
            Logo = seller.SellerInfo?.LogoUrl,
            SellerSubscriptionDto = activeSub != null ? new SellerSubscriptionDto
            {
                Id = activeSub.Id,
                StartDate = activeSub.StartDate,
                ExpiryDate = activeSub.ExpiryDate,
                EndDate = activeSub.ExpiryDate,
                SubscriptionPlanName = activeSub.SubscriptionPlan?.Name,
                Name = activeSub.SubscriptionPlan?.Name
            } : null,
            SellerInfo = seller.SellerInfo != null ? new SellerInfoDto
            {
                LogoUrl = seller.SellerInfo.LogoUrl,
                PhoneNumber = seller.SellerInfo.PhoneNumber,
                Email = seller.SellerInfo.Email,
                WebSiteUrl = seller.SellerInfo.WebSiteUrl,
                TwitterUrl = seller.SellerInfo.TwitterUrl,
                FacebookUrl = seller.SellerInfo.FacebookUrl,
                InstagramUrl = seller.SellerInfo.InstagramUrl
            } : null
        };

        // SellerInfo varsa contact bilgilerini de ana DTO'ya ekle (geriye dönük uyumluluk)
        if (seller.SellerInfo != null)
        {
            sellerDto.PhoneNumber = seller.SellerInfo.PhoneNumber;
            sellerDto.Email = seller.SellerInfo.Email;
        }

        return new GetSellerByIdQueryResult
        {
            Seller = sellerDto
        }.ReturnOk();
    }
}
