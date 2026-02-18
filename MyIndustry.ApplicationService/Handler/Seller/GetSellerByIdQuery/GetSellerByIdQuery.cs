using MediatR;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerByIdQuery;

public record GetSellerByIdQuery : IRequest<GetSellerByIdQueryResult>
{
    public Guid SellerId { get; set; }
}
