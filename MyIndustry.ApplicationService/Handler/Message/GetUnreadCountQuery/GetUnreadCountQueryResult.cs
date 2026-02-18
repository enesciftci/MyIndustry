namespace MyIndustry.ApplicationService.Handler.Message.GetUnreadCountQuery;

public record GetUnreadCountQueryResult : ResponseBase
{
    public int UnreadCount { get; set; }
}
