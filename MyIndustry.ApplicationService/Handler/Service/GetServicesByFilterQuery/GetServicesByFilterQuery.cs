namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public sealed record GetServicesByFilterQuery : IRequest<GetServicesByFilterQueryResult>
{
    public Guid? CategoryId { get; set; }
    
    public string? SearchTerm { get; set; }

    public int? CityId { get; set; }

    public int? DistrictId { get; set; }
}