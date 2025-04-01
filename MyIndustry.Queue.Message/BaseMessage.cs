namespace MyIndustry.Queue.Message;

public abstract class BaseMessage
{
    public Guid MessageId { get; set; }
    public DateTime MessageDate { get; set; }
}