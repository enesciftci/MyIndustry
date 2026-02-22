using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;

public record GetSupportTicketsQueryResult : ResponseBase
{
    public List<SupportTicketDto> Tickets { get; set; }
    public int TotalCount { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
}
