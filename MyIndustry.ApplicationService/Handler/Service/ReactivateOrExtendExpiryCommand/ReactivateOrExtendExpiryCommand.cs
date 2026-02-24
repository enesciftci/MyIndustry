namespace MyIndustry.ApplicationService.Handler.Service.ReactivateOrExtendExpiryCommand;

public sealed record ReactivateOrExtendExpiryCommand : IRequest<ReactivateOrExtendExpiryCommandResult>
{
    public Guid ServiceId { get; set; }
    public Guid SellerId { get; set; }
}
