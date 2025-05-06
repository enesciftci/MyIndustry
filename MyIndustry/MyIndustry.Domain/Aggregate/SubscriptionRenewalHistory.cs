namespace MyIndustry.Domain.Aggregate;

public class SubscriptionRenewalHistory : Entity
{
    public Guid SellerId { get; set; }
    public DateTime RenewedAt { get; set; }
    public string PaymentProviderTransactionId { get; set; } // Stripe/Iyzico ID
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public ICollection<Seller> Sellers { get; set; }
}