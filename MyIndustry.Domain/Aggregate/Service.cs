namespace MyIndustry.Domain.Aggregate;

// verilen servis ya da hizmet
public class Service : Entity
{
    public decimal Price { get; set; }
    public string Description { get; set; }
    public int EstimatedEndDay { get; set; }
    public string ImageUrls { get; set; }
    public Guid SellerId { get; set; }
    public bool IsActive { get; set; }
}