using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;

public record GetServicesByIdQueryResult : ResponseBase
{
    public ServiceDto Service { get; set; }
}