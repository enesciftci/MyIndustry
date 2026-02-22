using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.Domain.Aggregate;

// satıcı
public class Seller : Entity
{
    public string IdentityNumber { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public SellerSector Sector { get; set; }
    public string AgreementUrl { get; set; }
    public bool IsActive { get; set; }
    public SellerInfo SellerInfo { get; set; }
    // public ICollection<Contract> Contracts { get; set; }
    /// <summary>Geçmiş dahil tüm abonelik kayıtları. Aktif olan: FirstOrDefault(s => s.IsActive)</summary>
    public ICollection<SellerSubscription> SellerSubscriptions { get; set; }
    public ICollection<Service> Services { get; set; }
    public ICollection<CampaignUsage> CampaignUsages { get; set; }
    public ICollection<SubscriptionRenewalHistory> SubscriptionRenewalHistories { get; set; }
    public ICollection<Address> Addresses { get; set; }
}