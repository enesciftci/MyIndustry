namespace MyIndustry.ApplicationService.Handler.Service.GetServiceBySlugQuery;

public record GetServiceBySlugQuery : IRequest<GetServiceBySlugQueryResult>
{
    public string Slug { get; set; }
}
