using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerQuery;

public record GetSellerQueryResult : ResponseBase
{
    public SellerDto Seller { get; set; }
};