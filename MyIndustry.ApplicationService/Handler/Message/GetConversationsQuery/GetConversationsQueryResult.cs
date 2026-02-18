using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Message.GetConversationsQuery;

public record GetConversationsQueryResult : ResponseBase
{
    public List<ConversationDto> Conversations { get; set; } = new();
}
