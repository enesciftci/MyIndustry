using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainTicket = MyIndustry.Domain.Aggregate.SupportTicket;

namespace MyIndustry.ApplicationService.Handler.SupportTicket.UpdateSupportTicketCommand;

public class UpdateSupportTicketCommandHandler : IRequestHandler<UpdateSupportTicketCommand, UpdateSupportTicketCommandResult>
{
    private readonly IGenericRepository<DomainTicket> _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupportTicketCommandHandler(
        IGenericRepository<DomainTicket> ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateSupportTicketCommandResult> Handle(UpdateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (ticket == null)
        {
            return new UpdateSupportTicketCommandResult().ReturnNotFound("Destek talebi bulunamadı.");
        }

        var oldStatus = ticket.Status;
        
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.AdminNotes = request.AdminNotes;
        
        // If admin response is provided and changed
        if (!string.IsNullOrWhiteSpace(request.AdminResponse) && request.AdminResponse != ticket.AdminResponse)
        {
            ticket.AdminResponse = request.AdminResponse;
            ticket.RespondedDate = DateTime.UtcNow;
        }

        // Set closed date if status changed to Closed
        if (request.Status == TicketStatus.Closed && oldStatus != TicketStatus.Closed)
        {
            ticket.ClosedDate = DateTime.UtcNow;
        }

        ticket.ModifiedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateSupportTicketCommandResult().ReturnOk("Destek talebi güncellendi.");
    }
}
