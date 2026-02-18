using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerProfileQuery;

public record GetSellerProfileQueryResult : ResponseBase
{
    public SellerProfileDto Seller { get; set; }
}

public class SellerProfileDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string IdentityNumber { get; set; }
    public int Sector { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // SellerInfo
    public string LogoUrl { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string TwitterUrl { get; set; }
    public string FacebookUrl { get; set; }
    public string InstagramUrl { get; set; }
    public string WebSiteUrl { get; set; }
    
    // Statistics
    public int TotalServices { get; set; }
    public int ActiveServices { get; set; }
    public int TotalViews { get; set; }
    public int TotalFavorites { get; set; }
    
    // Subscription
    public SellerSubscriptionDto Subscription { get; set; }
}
