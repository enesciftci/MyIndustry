using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;

namespace MyIndustry.ApplicationService.Handler.Message.GetUnreadCountQuery;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, GetUnreadCountQueryResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;

    public GetUnreadCountQueryHandler(IGenericRepository<DomainMessage> messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<GetUnreadCountQueryResult> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var unreadCount = await _messageRepository
            .GetAllQuery()
            .CountAsync(m => m.ReceiverId == request.UserId && !m.IsRead, cancellationToken);

        return new GetUnreadCountQueryResult
        {
            UnreadCount = unreadCount
        }.ReturnOk();
    }
}
