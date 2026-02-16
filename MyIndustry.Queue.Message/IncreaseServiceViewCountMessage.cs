namespace MyIndustry.Queue.Message;

public record IncreaseServiceViewCountMessage : BaseMessage
{
    public Guid ServiceId { get; set; }
}