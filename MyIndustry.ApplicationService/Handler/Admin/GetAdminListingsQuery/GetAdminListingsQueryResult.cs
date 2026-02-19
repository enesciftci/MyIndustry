using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Admin.GetAdminListingsQuery;

public record GetAdminListingsQueryResult : ResponseBase
{
    public List<AdminListingDto> Listings { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
