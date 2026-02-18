using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Message.GetConversationMessagesQuery;

public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, GetConversationMessagesQueryResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IGenericRepository<DomainService> _serviceRepository;

    public GetConversationMessagesQueryHandler(
        IGenericRepository<DomainMessage> messageRepository,
        IGenericRepository<DomainService> serviceRepository)
    {
        _messageRepository = messageRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<GetConversationMessagesQueryResult> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
        {
            return new GetConversationMessagesQueryResult().ReturnNotFound("İlan bulunamadı.");
        }

        // Get messages between the two users about this service
        var messages = await _messageRepository
            .GetAllQuery()
            .Where(m => m.ServiceId == request.ServiceId &&
                       ((m.SenderId == request.UserId && m.ReceiverId == request.OtherUserId) ||
                        (m.SenderId == request.OtherUserId && m.ReceiverId == request.UserId)))
            .OrderBy(m => m.CreatedDate)
            .ToListAsync(cancellationToken);

        var otherUserMessage = messages.FirstOrDefault(m => m.SenderId == request.OtherUserId);

        var messageDtos = messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ServiceId = m.ServiceId,
            ServiceTitle = service.Title,
            SenderId = m.SenderId,
            SenderName = m.SenderName,
            SenderEmail = m.SenderEmail,
            ReceiverId = m.ReceiverId,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedDate = m.CreatedDate,
            IsFromCurrentUser = m.SenderId == request.UserId
        }).ToList();

        return new GetConversationMessagesQueryResult
        {
            ServiceTitle = service.Title,
            ServiceImageUrl = service.ImageUrls?.Split(',').FirstOrDefault(),
            OtherUserName = otherUserMessage?.SenderName ?? "Kullanıcı",
            Messages = messageDtos
        }.ReturnOk();
    }
}
