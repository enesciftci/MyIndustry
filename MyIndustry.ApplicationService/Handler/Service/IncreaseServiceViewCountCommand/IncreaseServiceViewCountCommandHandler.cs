namespace MyIndustry.ApplicationService.Handler.Service.IncreaseServiceViewCountCommand;

public sealed class IncreaseServiceViewCountCommandHandler : IRequestHandler<IncreaseServiceViewCountCommand, IncreaseServiceViewCountCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IncreaseServiceViewCountCommandHandler(IUnitOfWork unitOfWork, IGenericRepository<Domain.Aggregate.Service> serviceRepository)
    {
        _unitOfWork = unitOfWork;
        _serviceRepository = serviceRepository;
    }

    public async Task<IncreaseServiceViewCountCommandResult> Handle(IncreaseServiceViewCountCommand request, CancellationToken cancellationToken)
    {
       var service = await _serviceRepository.GetById(request.ServiceId, cancellationToken);
       
       service.ViewCount++;
       
       _serviceRepository.Update(service);
       
       await _unitOfWork.SaveChangesAsync(cancellationToken);
       
       return new IncreaseServiceViewCountCommandResult().ReturnOk();
    }
}