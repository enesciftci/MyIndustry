namespace MyIndustry.ApplicationService.Handler.Purchaser.GetPurchaserQuery;

public sealed record GetPurchaserQuery : IRequest<GetPurchaserQueryResult>
{
    public Guid PurchaserId { get; set; }
}