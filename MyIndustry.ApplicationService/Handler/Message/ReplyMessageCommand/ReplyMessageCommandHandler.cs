using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Message.ReplyMessageCommand;

public class ReplyMessageCommandHandler : IRequestHandler<ReplyMessageCommand, ReplyMessageCommandResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReplyMessageCommandHandler(
        IGenericRepository<DomainMessage> messageRepository,
        IGenericRepository<DomainService> serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReplyMessageCommandResult> Handle(ReplyMessageCommand request, CancellationToken cancellationToken)
    {
        // Verify service exists
        var service = await _serviceRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
        {
            return new ReplyMessageCommandResult().ReturnNotFound("İlan bulunamadı.");
        }

        // Verify there's an existing conversation
        var existingMessage = await _messageRepository
            .GetAllQuery()
            .AnyAsync(m => m.ServiceId == request.ServiceId &&
                          ((m.SenderId == request.UserId && m.ReceiverId == request.ReceiverId) ||
                           (m.SenderId == request.ReceiverId && m.ReceiverId == request.UserId)), 
                      cancellationToken);

        if (!existingMessage)
        {
            return new ReplyMessageCommandResult().ReturnBadRequest("Bu konuşmada yanıt veremezsiniz.");
        }

        var message = new DomainMessage
        {
            Id = Guid.NewGuid(),
            ServiceId = request.ServiceId,
            SenderId = request.UserId,
            ReceiverId = request.ReceiverId,
            SenderName = request.UserName,
            SenderEmail = request.UserEmail,
            Content = request.Content,
            IsRead = false
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReplyMessageCommandResult
        {
            MessageId = message.Id
        }.ReturnOk("Yanıtınız gönderildi.");
    }
}
