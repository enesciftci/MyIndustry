namespace MyIndustry.Domain.Aggregate;

public class SupportTicket : Entity
{
    /// <summary>
    /// User ID if logged in (nullable for anonymous submissions)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// User type: 0=Anonymous, 1=Purchaser, 2=Seller
    /// </summary>
    public int UserType { get; set; }
    
    /// <summary>
    /// Contact name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Contact email
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Contact phone (optional)
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Ticket subject
    /// </summary>
    public string Subject { get; set; }
    
    /// <summary>
    /// Ticket message/description
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Ticket category: General, Technical, Payment, Complaint, Suggestion, Other
    /// </summary>
    public TicketCategory Category { get; set; }
    
    /// <summary>
    /// Ticket status: Open, InProgress, Resolved, Closed
    /// </summary>
    public TicketStatus Status { get; set; }
    
    /// <summary>
    /// Priority: Low, Normal, High, Urgent
    /// </summary>
    public TicketPriority Priority { get; set; }
    
    /// <summary>
    /// Admin notes (internal)
    /// </summary>
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// Admin response to user
    /// </summary>
    public string? AdminResponse { get; set; }
    
    /// <summary>
    /// Date when admin responded
    /// </summary>
    public DateTime? RespondedDate { get; set; }
    
    /// <summary>
    /// Date when ticket was closed
    /// </summary>
    public DateTime? ClosedDate { get; set; }
}

public enum TicketCategory
{
    General = 0,
    Technical = 1,
    Payment = 2,
    Complaint = 3,
    Suggestion = 4,
    Other = 5
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}

public enum TicketPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
