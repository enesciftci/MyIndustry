using MediatR;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Handler.SupportTicket.UpdateSupportTicketCommand;

public record UpdateSupportTicketCommand : IRequest<UpdateSupportTicketCommandResult>
{
    public Guid Id { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string? AdminNotes { get; set; }
    public string? AdminResponse { get; set; }
}
