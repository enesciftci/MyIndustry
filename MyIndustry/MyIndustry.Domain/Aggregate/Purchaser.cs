namespace MyIndustry.Domain.Aggregate;

// alıcı müşteri
public class Purchaser : Entity
{
    public bool IsActive { get; set; }
    public ICollection<Address> Addresses { get; set; }
    public PurchaserInfo PurchaserInfo { get; set; }
    // public ICollection<Contract> Contracts { get; set; }
}