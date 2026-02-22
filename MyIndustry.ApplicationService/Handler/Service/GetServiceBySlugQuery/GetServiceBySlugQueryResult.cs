using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServiceBySlugQuery;

public record GetServiceBySlugQueryResult : ResponseBase
{
    public ServiceDto Service { get; set; }
}
