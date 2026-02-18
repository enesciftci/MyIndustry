namespace MyIndustry.ApplicationService.Dto;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsFromCurrentUser { get; set; }
}

public class ConversationDto
{
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; }
    public string ServiceImageUrl { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; }
    public string OtherUserEmail { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageDate { get; set; }
    public int UnreadCount { get; set; }
}
