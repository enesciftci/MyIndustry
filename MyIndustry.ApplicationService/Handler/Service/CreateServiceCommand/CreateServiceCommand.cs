using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;

public record CreateServiceCommand : IRequest<CreateServiceCommandResult>
{
    public int Price { get; set; }
    public string Description { get; set; }
    public int EstimatedEndDay { get; set; }
    public string ImageUrls { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; }
    
    // Location fields
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Neighborhood { get; set; }
    
    // Product condition and listing type
    public ProductCondition Condition { get; set; } = ProductCondition.New;
    public ListingType ListingType { get; set; } = ListingType.ForSale;
}