using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;

namespace MyIndustry.ApplicationService.Handler.Message.MarkMessagesAsReadCommand;

public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, MarkMessagesAsReadCommandResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkMessagesAsReadCommandHandler(
        IGenericRepository<DomainMessage> messageRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
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

        if (unreadMessages.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new MarkMessagesAsReadCommandResult
        {
            MarkedCount = unreadMessages.Count
        }.ReturnOk();
    }
}
