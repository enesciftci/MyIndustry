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
            .Include(s => s.SellerSubscription)
                .ThenInclude(ss => ss.SubscriptionPlan)
            .Include(s => s.SellerInfo)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == request.SellerId, cancellationToken);

        if (seller == null)
        {
            return new GetSellerByIdQueryResult().ReturnNotFound("Satıcı bulunamadı.");
        }

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
            SellerSubscriptionDto = seller.SellerSubscription != null ? new SellerSubscriptionDto
            {
                Id = seller.SellerSubscription.Id,
                StartDate = seller.SellerSubscription.StartDate,
                ExpiryDate = seller.SellerSubscription.ExpiryDate,
                EndDate = seller.SellerSubscription.ExpiryDate,
                SubscriptionPlanName = seller.SellerSubscription.SubscriptionPlan?.Name,
                Name = seller.SellerSubscription.SubscriptionPlan?.Name
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
