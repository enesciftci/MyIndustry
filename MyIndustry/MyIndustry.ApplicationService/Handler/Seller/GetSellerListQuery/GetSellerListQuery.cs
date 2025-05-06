namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;

public record GetSellerListQuery : IRequest<GetSellerListQueryResult>
{
    public Pager Pager { get; set; }
}