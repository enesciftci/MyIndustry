namespace MyIndustry.ApplicationService.Dto;

public class ServiceDto
{
    public Guid Id { get; set; }
    public int Price { get; set; }
    public string Description { get; set; }
    public int EstimatedEndDay { get; set; }
    public string[] ImageUrls { get; set; }
    public Guid SellerId { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public string Title { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public SellerDto Seller { get; set; }
    public List<CategoryBreadcrumbDto> CategoryBreadcrumbs { get; set; }
    
    // Location fields
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Neighborhood { get; set; }
    
    // Product condition and listing type
    public int Condition { get; set; }      // 0=Sıfır, 1=İkinci El
    public int ListingType { get; set; }    // 0=Satılık, 1=Kiralık
}