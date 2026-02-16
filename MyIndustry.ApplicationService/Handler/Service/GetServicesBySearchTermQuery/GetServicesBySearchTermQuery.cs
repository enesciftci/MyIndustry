namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;

public class GetServicesBySearchTermQuery : IRequest<GetServicesBySearchTermQueryResult>
{
    public string Query { get; set; }
    public Pager Pager { get; set; }
}