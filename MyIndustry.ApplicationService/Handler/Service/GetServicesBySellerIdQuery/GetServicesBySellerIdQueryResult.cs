using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;

public record GetServicesBySellerIdQueryResult : ResponseBase
{
    public List<ServiceDto> Services { get; set; }
}

