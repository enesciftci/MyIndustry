using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Message.GetConversationMessagesQuery;

public record GetConversationMessagesQueryResult : ResponseBase
{
    public string ServiceTitle { get; set; }
    public string ServiceImageUrl { get; set; }
    public string OtherUserName { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
}
