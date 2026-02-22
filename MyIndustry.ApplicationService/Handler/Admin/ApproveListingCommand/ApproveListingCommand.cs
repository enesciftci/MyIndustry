using MediatR;
using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;

public record ApproveListingCommand : IRequest<ApproveListingCommandResult>
{
    public Guid ServiceId { get; set; }
    public bool Approve { get; set; } // true = approve, false = reject
    public RejectionReasonType? RejectionReasonType { get; set; }
    public string? RejectionReasonDescription { get; set; }
}
