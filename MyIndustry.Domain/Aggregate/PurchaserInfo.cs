namespace MyIndustry.Domain.Aggregate;

public class PurchaserInfo : Entity
{
    public Guid PurchaserId { get; set; }
    public string Address { get; set; }
    public int CityId { get; set; }
}