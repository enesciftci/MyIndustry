using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Admin.GetAdminStatsQuery;

public record GetAdminStatsQueryResult : ResponseBase
{
    public AdminStatsDto Stats { get; set; }
}
