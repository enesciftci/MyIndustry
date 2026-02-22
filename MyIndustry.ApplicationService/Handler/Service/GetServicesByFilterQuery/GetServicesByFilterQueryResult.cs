using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public sealed record GetServicesByFilterQueryResult : ResponseBase
{
    public List<ServiceDto> Services { get; set; }
    public int TotalCount { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
}