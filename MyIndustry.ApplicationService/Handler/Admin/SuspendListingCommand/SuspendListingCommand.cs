using MediatR;
using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Admin.SuspendListingCommand;

public class SuspendListingCommand : IRequest<SuspendListingCommandResult>
{
    public Guid ServiceId { get; set; }
    public bool Suspend { get; set; } // true = suspend, false = unsuspend
    public SuspensionReasonType? SuspensionReasonType { get; set; }
    public string? SuspensionReasonDescription { get; set; }
}
