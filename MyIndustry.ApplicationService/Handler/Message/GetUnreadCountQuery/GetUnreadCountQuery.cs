using MediatR;

namespace MyIndustry.ApplicationService.Handler.Message.GetUnreadCountQuery;

public record GetUnreadCountQuery : IRequest<GetUnreadCountQueryResult>
{
    public Guid UserId { get; set; }
}
