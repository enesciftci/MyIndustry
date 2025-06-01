namespace MyIndustry.ApplicationService.Handler.Service.DeleteServiceByIdCommand;

public class DeleteServiceByIdCommandHandler : IRequestHandler<DeleteServiceByIdCommand, DeleteServiceByIdCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;

    public DeleteServiceByIdCommandHandler(IUnitOfWork unitOfWork,
        IGenericRepository<Domain.Aggregate.Service> serviceRepository)
    {
        _unitOfWork = unitOfWork;
        _serviceRepository = serviceRepository;
    }

    public async Task<DeleteServiceByIdCommandResult> Handle(DeleteServiceByIdCommand request,
        CancellationToken cancellationToken)
    {
        await _serviceRepository.Delete(p => p.Id == request.Id && p.SellerId == request.SellerId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeleteServiceByIdCommandResult().ReturnOk();
    }
}