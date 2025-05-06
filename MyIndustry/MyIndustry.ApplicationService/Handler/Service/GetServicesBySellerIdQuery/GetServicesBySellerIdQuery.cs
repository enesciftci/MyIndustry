namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;

public record GetServicesBySellerIdQuery : IRequest<GetServicesBySellerIdQueryResult>
{
    public Guid SellerId { get; set; }
    public Pager Pager { get; set; }
}