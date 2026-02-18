using MediatR;

namespace MyIndustry.ApplicationService.Handler.Message.GetConversationsQuery;

public record GetConversationsQuery : IRequest<GetConversationsQueryResult>
{
    public Guid UserId { get; set; }
}
