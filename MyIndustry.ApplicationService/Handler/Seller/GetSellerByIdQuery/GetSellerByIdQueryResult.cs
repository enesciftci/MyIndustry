using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerByIdQuery;

public record GetSellerByIdQueryResult : ResponseBase
{
    public SellerDto Seller { get; set; }
}
