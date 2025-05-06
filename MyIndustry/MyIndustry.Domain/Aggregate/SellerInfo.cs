namespace MyIndustry.Domain.Aggregate;

public class SellerInfo : Entity
{
    public Guid SellerId { get; set; }
    public string LogoUrl { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public Seller Seller { get; set; }
}