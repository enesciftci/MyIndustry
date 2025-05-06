namespace MyIndustry.Domain.Aggregate;

public class PurchaserInfo : Entity
{
    public Guid PurchaserId { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public Purchaser Purchaser { get; set; }
}