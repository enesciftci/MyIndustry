namespace MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand,CreateServiceCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;

    public CreateServiceCommandHandler(IGenericRepository<Domain.Aggregate.Service> serviceRepository, IUnitOfWork unitOfWork, IGenericRepository<Domain.Aggregate.Seller> sellerRepository)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _sellerRepository = sellerRepository;
    }

    public async Task<CreateServiceCommandResult> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetById(request.SellerId, cancellationToken);
        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı");
        
        await _serviceRepository.AddAsync(new Domain.Aggregate.Service()
        {
            Description = request.Description,
            Price = request.Price,
            SellerId = request.SellerId,
            ImageUrls = request.ImageUrls,
            EstimatedEndDay = request.EstimatedEndDay,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateServiceCommandResult().ReturnOk();
    }
}