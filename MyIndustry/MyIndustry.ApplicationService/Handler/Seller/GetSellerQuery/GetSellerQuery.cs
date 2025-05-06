namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerQuery;

public record GetSellerQuery : IRequest<GetSellerQueryResult>
{
    public Guid Id { get; set; }
}