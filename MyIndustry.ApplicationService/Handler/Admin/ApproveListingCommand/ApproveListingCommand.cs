using MediatR;

namespace MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;

public record ApproveListingCommand : IRequest<ApproveListingCommandResult>
{
    public Guid ServiceId { get; set; }
    public bool Approve { get; set; } // true = approve, false = reject
    public string? RejectionReason { get; set; }
}
