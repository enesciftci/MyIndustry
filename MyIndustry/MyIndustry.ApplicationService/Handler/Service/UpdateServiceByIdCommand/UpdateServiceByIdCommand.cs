using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;

public sealed record UpdateServiceByIdCommand : IRequest<UpdateServiceByIdCommandResult>
{
    public ServiceDto ServiceDto { get; set; }
}