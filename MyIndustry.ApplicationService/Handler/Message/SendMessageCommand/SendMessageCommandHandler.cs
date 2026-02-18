using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageCommandResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IGenericRepository<DomainService> _serviceRepository;

    public SendMessageCommandHandler(
        IGenericRepository<DomainMessage> messageRepository,
        IGenericRepository<DomainService> serviceRepository)
    {
        _messageRepository = messageRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<SendMessageCommandResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Get the service to find the seller (receiver)
        var service = await _serviceRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
        {
            return new SendMessageCommandResult().ReturnNotFound("İlan bulunamadı.");
        }

        // Sender cannot message themselves
        if (service.SellerId == request.SenderId)
        {
            return new SendMessageCommandResult().ReturnBadRequest("Kendi ilanınıza mesaj gönderemezsiniz.");
        }

        var message = new DomainMessage
        {
            Id = Guid.NewGuid(),
            ServiceId = request.ServiceId,
            SenderId = request.SenderId,
            ReceiverId = service.SellerId,
            SenderName = request.SenderName,
            SenderEmail = request.SenderEmail,
            Content = request.Content,
            IsRead = false
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        return new SendMessageCommandResult
        {
            MessageId = message.Id
        }.ReturnOk("Mesajınız gönderildi.");
    }
}
