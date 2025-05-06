using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;

public sealed record UpdateServiceByIdCommand(ServiceDto ServiceDto) : IRequest<UpdateServiceByIdCommandResult>;