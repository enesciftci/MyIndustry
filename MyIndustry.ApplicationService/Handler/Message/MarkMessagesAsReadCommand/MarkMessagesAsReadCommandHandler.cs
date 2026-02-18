using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;

namespace MyIndustry.ApplicationService.Handler.Message.MarkMessagesAsReadCommand;

public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, MarkMessagesAsReadCommandResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;

    public MarkMessagesAsReadCommandHandler(IGenericRepository<DomainMessage> messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<MarkMessagesAsReadCommandResult> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        // Mark all unread messages in this conversation as read
        var unreadMessages = await _messageRepository
            .GetAllQuery()
            .Where(m => m.ServiceId == request.ServiceId &&
                       m.SenderId == request.OtherUserId &&
                       m.ReceiverId == request.UserId &&
                       !m.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            _messageRepository.Update(message);
        }

        return new MarkMessagesAsReadCommandResult
        {
            MarkedCount = unreadMessages.Count
        }.ReturnOk();
    }
}
