using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Purchaser.GetPurchaserQuery;

public sealed record GetPurchaserQueryResult : ResponseBase
{
    public PurchaserDto PurchaserDto { get; set; }
}