using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;

public record GetServicesByRandomlyQueryResult : PagerResponseBase
{
    public List<ServiceDto> Services { get; set; }
}