using MediatR;

namespace MyIndustry.ApplicationService.Handler.Admin.SuspendListingCommand;

public class SuspendListingCommand : IRequest<SuspendListingCommandResult>
{
    public Guid ServiceId { get; set; }
    public bool Suspend { get; set; } // true = suspend, false = unsuspend
    public string? Reason { get; set; }
}
