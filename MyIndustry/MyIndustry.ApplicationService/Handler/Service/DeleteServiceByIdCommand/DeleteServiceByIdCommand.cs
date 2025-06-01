namespace MyIndustry.ApplicationService.Handler.Service.DeleteServiceByIdCommand;

public sealed record DeleteServiceByIdCommand : IRequest<DeleteServiceByIdCommandResult>
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
}