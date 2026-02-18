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
            SellerSubscriptionDto = seller.SellerSubscription != null ? new SellerSubscriptionDto
            {
                Id = seller.SellerSubscription.Id,
                StartDate = seller.SellerSubscription.StartDate,
                ExpiryDate = seller.SellerSubscription.ExpiryDate,
                EndDate = seller.SellerSubscription.ExpiryDate, // Use ExpiryDate for EndDate
                SubscriptionPlanName = seller.SellerSubscription.SubscriptionPlan?.Name,
                Name = seller.SellerSubscription.SubscriptionPlan?.Name
            } : null
        };

        // SellerInfo varsa contact bilgilerini ekle
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
