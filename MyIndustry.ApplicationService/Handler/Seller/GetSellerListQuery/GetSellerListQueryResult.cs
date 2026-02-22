using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;

public record GetSellerListQueryResult : ResponseBase
{
    public List<SellerDto> Sellers { get; set; }
    public int TotalCount { get; set; }
};