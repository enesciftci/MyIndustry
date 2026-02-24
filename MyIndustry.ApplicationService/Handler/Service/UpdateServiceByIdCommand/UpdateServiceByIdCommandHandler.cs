using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.ApplicationService.Helpers;

namespace MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;

public class UpdateServiceByIdCommandHandler : IRequestHandler<UpdateServiceByIdCommand, UpdateServiceByIdCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceByIdCommandHandler(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository, 
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IGenericRepository<Domain.Aggregate.Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _sellerRepository = sellerRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateServiceByIdCommandResult> Handle(UpdateServiceByIdCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository
            .GetAllQuery()
            .Include(s => s.Seller)
                .ThenInclude(s => s.SellerSubscriptions)
            .FirstOrDefaultAsync(p => p.Id == request.ServiceDto.Id && p.SellerId == request.ServiceDto.SellerId, cancellationToken);

        if (service is not { IsActive: true })
        {
            throw new BusinessRuleException("Servis bulunamadı.");
        }

        var activeSubscription = service.Seller?.SellerSubscriptions?.FirstOrDefault(ss => ss.IsActive);

        // Check if changing from non-featured to featured
        if (!service.IsFeatured && request.ServiceDto.IsFeatured)
        {
            if (activeSubscription == null)
                throw new BusinessRuleException("Abonelik planı bulunamadı.");
            if (activeSubscription.RemainingFeaturedQuota <= 0)
                throw new BusinessRuleException("Kalan öne çıkan ilan kotası dolu. Lütfen abonelik planınızı yükseltin.");
            activeSubscription.RemainingFeaturedQuota--;
        }
        else if (service.IsFeatured && !request.ServiceDto.IsFeatured)
        {
            if (activeSubscription != null)
                activeSubscription.RemainingFeaturedQuota++;
        }
        
        service.Title = request.ServiceDto.Title;
        service.Description = request.ServiceDto.Description;
        if (request.ServiceDto.ImageUrls != null)
        {
            service.ImageUrls = request.ServiceDto.ImageUrls.Length > 0
                ? System.Text.Json.JsonSerializer.Serialize(request.ServiceDto.ImageUrls)
                : string.Empty;
        }
        service.Price = new Amount(request.ServiceDto.Price).ToDecimal();
        service.EstimatedEndDay = request.ServiceDto.EstimatedEndDay;
        service.IsFeatured = request.ServiceDto.IsFeatured;

        if (request.ServiceDto.CategoryId != Guid.Empty)
        {
            var categoryExists = await _categoryRepository.GetAllQuery()
                .AnyAsync(c => c.Id == request.ServiceDto.CategoryId && c.IsActive, cancellationToken);
            if (!categoryExists)
                throw new BusinessRuleException("Geçersiz kategori.");
            service.CategoryId = request.ServiceDto.CategoryId;
        }

        // Update SEO fields if title changed
        if (service.Title != request.ServiceDto.Title || string.IsNullOrEmpty(service.Slug))
        {
            var baseSlug = SlugHelper.GenerateSlug(request.ServiceDto.Title);
            var uniqueSlug = await SlugHelper.GenerateUniqueSlugAsync(
                baseSlug,
                async (slug) => await _serviceRepository
                    .GetAllQuery()
                    .AnyAsync(s => s.Slug == slug && s.Id != service.Id, cancellationToken)
            );
            service.Slug = uniqueSlug;
        }
        
        // Update meta fields
        var listingTypeText = request.ServiceDto.ListingType == 0 ? "Satılık" : "Kiralık"; // 0=ForSale, 1=ForRent
        service.MetaTitle = $"{request.ServiceDto.Title} - {listingTypeText} | MyIndustry";
        var desc = request.ServiceDto.Description ?? "";
        service.MetaDescription = desc.Length > 160 ? desc.Substring(0, 157) + "..." : desc;

        _serviceRepository.Update(service);
        if (service.Seller != null)
        {
            _sellerRepository.Update(service.Seller);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateServiceByIdCommandResult().ReturnOk();
    }
}