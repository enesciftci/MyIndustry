using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.ApplicationService.Helpers;

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
            .Include(p => p.SellerSubscriptions!)
                .ThenInclude(ss => ss.SubscriptionPlan)
            .FirstOrDefaultAsync(p => p.Id == request.SellerId, cancellationToken);

        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı");

        var activeSubscription = seller.SellerSubscriptions?.FirstOrDefault(s => s.IsActive);
        if (activeSubscription == null)
            throw new BusinessRuleException("Abonelik planı bulunamadı. Lütfen abone olun.");
        
        if (activeSubscription.RemainingPostQuota <= 0)
            throw new BusinessRuleException("Kalan ilan kotası dolu. Lütfen abonelik planınızı yükseltin.");

        var plan = activeSubscription.SubscriptionPlan;
        var postDurationDays = plan?.PostDurationInDays ?? 365; // Varsayılan 1 yıl
        var expiryDate = DateTime.UtcNow.AddDays(postDurationDays);

        // Check featured quota if trying to create featured listing
        if (request.IsFeatured)
        {
            if (activeSubscription.RemainingFeaturedQuota <= 0)
                throw new BusinessRuleException("Kalan öne çıkan ilan kotası dolu. Lütfen abonelik planınızı yükseltin.");
        }

        var subCategoryExists = await _categoryRepository
            .AnyAsync(x => x.Id == request.CategoryId && x.IsActive, cancellationToken);

        if (!subCategoryExists)
            throw new BusinessRuleException("Alt kategori bulunamadı.");
        
        // Generate SEO slug from title
        var baseSlug = SlugHelper.GenerateSlug(request.Title);
        var uniqueSlug = await SlugHelper.GenerateUniqueSlugAsync(
            baseSlug,
            async (slug) => await _serviceRepository
                .GetAllQuery()
                .AnyAsync(s => s.Slug == slug, cancellationToken)
        );

        // Generate meta title and description
        var listingTypeText = request.ListingType == ListingType.ForSale ? "Satılık" : "Kiralık";
        var metaTitle = $"{request.Title} - {listingTypeText} | MyIndustry";
        var metaDescription = request.Description.Length > 160 
            ? request.Description.Substring(0, 157) + "..." 
            : request.Description;
        
        // Generate keywords from title and category
        var category = await _categoryRepository.GetById(request.CategoryId, cancellationToken);
        var keywords = new List<string> { request.Title };
        if (category != null) keywords.Add(category.Name);
        if (request.City != null) keywords.Add(request.City);
        keywords.Add("satılık");
        keywords.Add("myindustry");
        var metaKeywords = string.Join(", ", keywords);
        
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
            ExpiryDate = expiryDate, // İlan süresi paketteki PostDurationInDays ile belirlenir
            City = request.City,
            District = request.District,
            Neighborhood = request.Neighborhood,
            Condition = request.Condition,
            ListingType = request.ListingType,
            IsFeatured = request.IsFeatured,
            // SEO fields
            Slug = uniqueSlug,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords
        }, cancellationToken);

        activeSubscription.DecreaseRemainingPostQuota();
        if (request.IsFeatured)
            activeSubscription.RemainingFeaturedQuota--;

        _sellerSubscriptionRepository.Update(activeSubscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateServiceCommandResult().ReturnOk();
    }
}