using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.Domain.Aggregate;

// verilen servis ya da hizmet
public class Service : Entity
{
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public int EstimatedEndDay { get; set; }
    public string ImageUrls { get; set; }
    public Guid SellerId { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsApproved { get; set; }
    public Guid ApprovedBy { get; set; }
    
    // Location fields
    public string? City { get; set; }          // Şehir
    public string? District { get; set; }       // İlçe
    public string? Neighborhood { get; set; }   // Mahalle
    
    // Product condition and listing type
    public ProductCondition Condition { get; set; } = ProductCondition.New;  // Sıfır/İkinci El
    public ListingType ListingType { get; set; } = ListingType.ForSale;      // Satılık/Kiralık
    
    // Suspension
    public SuspensionReasonType? SuspensionReasonType { get; set; }
    public string? SuspensionReason { get; set; } // Kept for backward compatibility
    public string? SuspensionReasonDescription { get; set; }
    
    // Rejection
    public RejectionReasonType? RejectionReasonType { get; set; }
    public string? RejectionReasonDescription { get; set; }
    
    public Category Category { get; set; }
    public ICollection<ServiceViewLog> ServiceViewLogs { get; set; }
    public ICollection<Favorite> Favorites { get; set; }
    public Seller Seller { get; set; }
}