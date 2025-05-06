using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;

public record GetServicesByRandomlyQueryResult : ResponseBase
{
    public List<ServiceDto> Services { get; set; }
}