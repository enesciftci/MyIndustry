using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageCommandResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
        IGenericRepository<DomainMessage> messageRepository,
        IGenericRepository<DomainService> serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendMessageCommandResult
        {
            MessageId = message.Id
        }.ReturnOk("Mesajınız gönderildi.");
    }
}
