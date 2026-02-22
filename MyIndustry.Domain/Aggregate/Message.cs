namespace MyIndustry.Domain.Aggregate;

public class Message : Entity
{
    /// <summary>
    /// The service/listing this message is about
    /// </summary>
    public Guid ServiceId { get; set; }
    public virtual Service Service { get; set; }
    
    /// <summary>
    /// The user who sent the message (can be buyer or seller)
    /// </summary>
    public Guid SenderId { get; set; }
    
    /// <summary>
    /// The user who receives the message
    /// </summary>
    public Guid ReceiverId { get; set; }
    
    /// <summary>
    /// Message content
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// Whether the message has been read by receiver
    /// </summary>
    public bool IsRead { get; set; }
    
    /// <summary>
    /// Sender's name (cached for display)
    /// </summary>
    public string SenderName { get; set; }
    
    /// <summary>
    /// Sender's email (cached for display)
    /// </summary>
    public string SenderEmail { get; set; }
    
    /// <summary>
    /// Receiver's name (cached for display)
    /// </summary>
    public string? ReceiverName { get; set; }
    
    /// <summary>
    /// Receiver's email (cached for display)
    /// </summary>
    public string? ReceiverEmail { get; set; }
}
