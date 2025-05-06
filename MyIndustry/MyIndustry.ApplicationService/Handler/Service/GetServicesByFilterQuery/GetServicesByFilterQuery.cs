namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public sealed record GetServicesByFilterQuery : IRequest<GetServicesByFilterQueryResult>
{
    public Guid CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
}