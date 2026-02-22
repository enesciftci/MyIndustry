using MediatR;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainTicket = MyIndustry.Domain.Aggregate.SupportTicket;

namespace MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;

public class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, CreateSupportTicketCommandResult>
{
    private readonly IGenericRepository<DomainTicket> _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupportTicketCommandHandler(
        IGenericRepository<DomainTicket> ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateSupportTicketCommandResult> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = new DomainTicket
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            UserType = request.UserType,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Subject = request.Subject,
            Message = request.Message,
            Category = request.Category,
            Status = TicketStatus.Open,
            Priority = TicketPriority.Normal
        };

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSupportTicketCommandResult
        {
            TicketId = ticket.Id
        }.ReturnOk("Destek talebiniz başarıyla oluşturuldu. En kısa sürede size dönüş yapılacaktır.");
    }
}
