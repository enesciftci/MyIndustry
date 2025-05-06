using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public sealed record GetServicesByFilterQueryResult : ResponseBase
{
    public List<ServiceDto> Services { get; set; }
}