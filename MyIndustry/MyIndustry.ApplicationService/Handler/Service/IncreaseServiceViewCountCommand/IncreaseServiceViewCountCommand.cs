namespace MyIndustry.ApplicationService.Handler.Service.IncreaseServiceViewCountCommand;

public record IncreaseServiceViewCountCommand : IRequest<IncreaseServiceViewCountCommandResult>
{
    public Guid ServiceId { get; set; }
}