namespace MyIndustry.ApplicationService.Handler.Contract.CreateContractCommand;

public sealed class CreateContractCommandHandler : IRequestHandler<CreateContractCommand,CreateContractCommandResult>
{
    public Task<CreateContractCommandResult> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}