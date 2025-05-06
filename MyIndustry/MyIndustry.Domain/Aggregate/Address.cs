namespace MyIndustry.Domain.Aggregate;

public class Address : Entity
{
    public Guid UserId { get; set; }
    public string FullAddress { get; set; }
    public int City { get; set; }
    public int District { get; set; }
    public bool IsMain { get; set; }
    public bool IsActive { get; set; }
    public Seller Seller { get; set; }
    public Purchaser Purchaser { get; set; }
}