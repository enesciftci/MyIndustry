using MediatR;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;

public record CreateSupportTicketCommand : IRequest<CreateSupportTicketCommandResult>
{
    public Guid? UserId { get; set; }
    public int UserType { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public TicketCategory Category { get; set; }
}
