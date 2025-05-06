namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;

public class GetServicesByIdQuery : IRequest<GetServicesByIdQueryResult>
{
    public Guid Id { get; set; }
}