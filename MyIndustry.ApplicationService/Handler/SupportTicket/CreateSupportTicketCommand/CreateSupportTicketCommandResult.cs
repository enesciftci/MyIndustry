namespace MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;

public record CreateSupportTicketCommandResult : ResponseBase
{
    public Guid TicketId { get; set; }
}
