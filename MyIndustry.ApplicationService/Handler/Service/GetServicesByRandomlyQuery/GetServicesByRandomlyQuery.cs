namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;

public class GetServicesByRandomlyQuery : IRequest<GetServicesByRandomlyQueryResult>
{
    public Pager Pager { get; set; }
}