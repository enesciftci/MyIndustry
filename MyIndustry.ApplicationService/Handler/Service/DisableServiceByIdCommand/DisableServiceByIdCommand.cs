namespace MyIndustry.ApplicationService.Handler.Service.DisableServiceByIdCommand;

public sealed record DisableServiceByIdCommand : IRequest<DisableServiceByIdCommandResult>
{
   public Guid ServiceId { get; set; }
   public Guid SellerId { get; set; }
}