using MediatR;

namespace MyIndustry.ApplicationService.Handler.Admin.GetAdminListingsQuery;

public record GetAdminListingsQuery : IRequest<GetAdminListingsQueryResult>
{
    public string? Status { get; set; } // "pending", "approved", "rejected", "all"
    public string? SearchTerm { get; set; }
    public int Index { get; set; } = 1;
    public int Size { get; set; } = 20;
}
