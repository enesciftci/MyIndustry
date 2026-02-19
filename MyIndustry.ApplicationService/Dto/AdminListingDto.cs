namespace MyIndustry.ApplicationService.Dto;

public class AdminListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string[] ImageUrls { get; set; }
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    // Seller info
    public Guid SellerId { get; set; }
    public string SellerName { get; set; }
    public string SellerEmail { get; set; }
    
    // Category info
    public Guid? CategoryId { get; set; }
    public string CategoryName { get; set; }
    
    // Computed status
    public string Status { get; set; } // "pending", "approved", "rejected"
}
