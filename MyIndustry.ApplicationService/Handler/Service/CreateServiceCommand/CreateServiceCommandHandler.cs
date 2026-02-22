using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand,CreateServiceCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<SubCategory> _subcategoryRepository;
    private readonly IGenericRepository<Domain.Aggregate.Category> _categoryRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription>  _sellerSubscriptionRepository;

    public CreateServiceCommandHandler(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository, 
        IUnitOfWork unitOfWork, 
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository, 
        IGenericRepository<SubCategory> subcategoryRepository, IGenericRepository<Domain.Aggregate.Category> categoryRepository, IGenericRepository<Domain.Aggregate.SellerSubscription> sellerSubscriptionRepository)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _sellerRepository = sellerRepository;
        _subcategoryRepository = subcategoryRepository;
        _categoryRepository = categoryRepository;
        _sellerSubscriptionRepository = sellerSubscriptionRepository;
    }

    public async Task<CreateServiceCommandResult> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository
            .GetAllQuery()
            .Include(p => p.SellerSubscription)
            .FirstOrDefaultAsync(p => p.Id == request.SellerId, cancellationToken);

        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı");

        if (seller.SellerSubscription == null)
            throw new BusinessRuleException("Abonelik planı bulunamadı. Lütfen abone olun.");
        
        if(seller.SellerSubscription.RemainingPostQuota <= 0)
            throw new BusinessRuleException("Kalan ilan kotası dolu. Lütfen abonelik planınızı yükseltin.");

        var subCategoryExists = await _categoryRepository
            .AnyAsync(x => x.Id == request.CategoryId && x.IsActive, cancellationToken);

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
            CategoryId = request.CategoryId,
            IsActive = true,
            City = request.City,
            District = request.District,
            Neighborhood = request.Neighborhood,
            Condition = request.Condition,
            ListingType = request.ListingType
        }, cancellationToken);

        seller.SellerSubscription.DecreaseRemainingPostQuota();
        _sellerRepository.Update(seller);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateServiceCommandResult().ReturnOk();
    }
}