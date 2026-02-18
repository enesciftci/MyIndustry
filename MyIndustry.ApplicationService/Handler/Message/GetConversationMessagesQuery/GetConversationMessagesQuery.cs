using MediatR;

namespace MyIndustry.ApplicationService.Handler.Message.GetConversationMessagesQuery;

public record GetConversationMessagesQuery : IRequest<GetConversationMessagesQueryResult>
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid OtherUserId { get; set; }
}
