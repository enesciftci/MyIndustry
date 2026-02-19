using MediatR;

namespace MyIndustry.ApplicationService.Handler.Admin.GetAdminStatsQuery;

public record GetAdminStatsQuery : IRequest<GetAdminStatsQueryResult>
{
}
