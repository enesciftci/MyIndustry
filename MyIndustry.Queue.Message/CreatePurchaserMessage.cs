namespace MyIndustry.Queue.Message;

public sealed record CreatePurchaserMessage : BaseMessage
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}