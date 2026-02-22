using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public sealed record GetServicesByFilterQuery : IRequest<GetServicesByFilterQueryResult>
{
    public Guid? CategoryId { get; set; }
    
    public string? SearchTerm { get; set; }

    // Legacy city/district IDs (for seller address filtering)
    public int? CityId { get; set; }
    public int? DistrictId { get; set; }
    
    // New location filters (for service location)
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Neighborhood { get; set; }
    
    // Product condition and listing type filters
    public ProductCondition? Condition { get; set; }
    public ListingType? ListingType { get; set; }
    
    // Price range filters
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // Pagination
    public int Index { get; set; } = 1;
    public int Size { get; set; } = 20;
}