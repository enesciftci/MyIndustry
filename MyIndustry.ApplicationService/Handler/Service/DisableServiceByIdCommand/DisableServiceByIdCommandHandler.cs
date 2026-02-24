using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Handler.Service.DisableServiceByIdCommand;

public class DisableServiceByIdCommandHandler : IRequestHandler<DisableServiceByIdCommand,DisableServiceByIdCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;

    public DisableServiceByIdCommandHandler(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository, 
        IUnitOfWork unitOfWork, 
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository, 
        IGenericRepository<SubCategory> subcategoryRepository)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _sellerRepository = sellerRepository;
    }

    public async Task<DisableServiceByIdCommandResult> Handle(DisableServiceByIdCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetById(request.ServiceId, cancellationToken);

        if (service == null || service.SellerId != request.SellerId)
            throw new BusinessRuleException("Servis bulunamadı.");
        if (!service.IsActive)
            throw new BusinessRuleException("İlan zaten pasif.");

        service.IsActive = false;
        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DisableServiceByIdCommandResult().ReturnOk();
    }
}