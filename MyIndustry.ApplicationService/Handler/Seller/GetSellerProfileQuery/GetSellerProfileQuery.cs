using MediatR;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerProfileQuery;

public sealed record GetSellerProfileQuery : IRequest<GetSellerProfileQueryResult>
{
    public Guid UserId { get; set; }
}
