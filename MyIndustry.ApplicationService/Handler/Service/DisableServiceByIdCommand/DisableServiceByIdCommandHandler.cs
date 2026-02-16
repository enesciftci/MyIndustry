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
        var seller = await _sellerRepository.GetById(request.SellerId, cancellationToken);
        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı");

        var service = await _serviceRepository.GetById(request.ServiceId, cancellationToken);

        if (service is not { IsActive: true })
        {
            throw new BusinessRuleException("Servis bulunamadı ya da zaten pasif");
        }
        
        seller.IsActive = false;
        
        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DisableServiceByIdCommandResult().ReturnOk();
    }
}