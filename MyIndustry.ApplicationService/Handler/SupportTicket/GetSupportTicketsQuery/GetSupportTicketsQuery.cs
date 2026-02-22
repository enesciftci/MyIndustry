using MediatR;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;

public record GetSupportTicketsQuery : IRequest<GetSupportTicketsQueryResult>
{
    public int Index { get; set; } = 1;
    public int Size { get; set; } = 20;
    public TicketStatus? Status { get; set; }
    public TicketCategory? Category { get; set; }
    public TicketPriority? Priority { get; set; }
}
