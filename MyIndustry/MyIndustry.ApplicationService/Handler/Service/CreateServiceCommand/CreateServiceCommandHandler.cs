using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand,CreateServiceCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<SubCategory> _subcategoryRepository;

    public CreateServiceCommandHandler(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository, 
        IUnitOfWork unitOfWork, 
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository, 
        IGenericRepository<SubCategory> subcategoryRepository)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _sellerRepository = sellerRepository;
        _subcategoryRepository = subcategoryRepository;
    }

    public async Task<CreateServiceCommandResult> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetById(request.SellerId, cancellationToken);
        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı");
        
        var subCategoryExists = await _subcategoryRepository
            .AnyAsync(x => x.Id == request.SubCategoryId, cancellationToken);

        if (!subCategoryExists)
            throw new BusinessRuleException("Alt kategori bulunamadı.");
        
        await _serviceRepository.AddAsync(new Domain.Aggregate.Service()
        {
            Title = request.Title,
            Description = request.Description,
            Price = new Amount(request.Price).ToDecimal(),
            SellerId = request.SellerId,
            ImageUrls = request.ImageUrls,
            EstimatedEndDay = request.EstimatedEndDay,
            SubCategoryId = request.SubCategoryId,
            CategoryId = request.CategoryId,
            IsActive = true
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateServiceCommandResult().ReturnOk();
    }
}