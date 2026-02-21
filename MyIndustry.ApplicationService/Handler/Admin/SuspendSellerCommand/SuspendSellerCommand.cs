using MediatR;

namespace MyIndustry.ApplicationService.Handler.Admin.SuspendSellerCommand;

public class SuspendSellerCommand : IRequest<SuspendSellerCommandResult>
{
    public Guid SellerId { get; set; }
    public bool Suspend { get; set; } // true = suspend, false = unsuspend
    public string? Reason { get; set; }
}
