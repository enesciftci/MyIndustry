using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;

public record GetServicesBySearchTermQueryResult : ResponseBase
{
    public List<ServiceDto> Services { get; set; }
    public int TotalCount { get; set; }
}