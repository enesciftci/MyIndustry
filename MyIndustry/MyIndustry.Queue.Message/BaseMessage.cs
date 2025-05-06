namespace MyIndustry.Queue.Message;

public abstract record BaseMessage
{
    public Guid MessageId { get; set; }
    public DateTime MessageDate { get; set; }
}