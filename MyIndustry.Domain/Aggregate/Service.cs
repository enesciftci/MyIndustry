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
    public Category Category { get; set; }
    public ICollection<ServiceViewLog> ServiceViewLogs { get; set; }
    public ICollection<Favorite> Favorites { get; set; }
    public Seller Seller { get; set; }
}