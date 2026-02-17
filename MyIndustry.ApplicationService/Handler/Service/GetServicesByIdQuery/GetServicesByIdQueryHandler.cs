using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;

public class GetServicesByIdQueryHandler : IRequestHandler<GetServicesByIdQuery,GetServicesByIdQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Category> _categoryRepository;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;

    public GetServicesByIdQueryHandler(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository,
        IGenericRepository<Domain.Aggregate.Category> categoryRepository,
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _sellerRepository = sellerRepository;
    }

    public async Task<GetServicesByIdQueryResult> Handle(GetServicesByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetById(request.Id, cancellationToken);

        if (service == null)
            throw new BusinessRuleException("Not found");

        // Get category breadcrumbs (from root to current category)
        var categoryBreadcrumbs = await GetCategoryBreadcrumbs(service.CategoryId, cancellationToken);

        // Get seller info
        var seller = await _sellerRepository.GetById(service.SellerId, cancellationToken);
        
        return new GetServicesByIdQueryResult()
        {
            Service = new ServiceDto()
            {
                Id = service.Id,
                Price = new Amount(service.Price).ToInt(),
                Description = service.Description,
                Title = service.Title,
                ImageUrls = service.ImageUrls?.Split(','),
                SellerId = service.SellerId,
                EstimatedEndDay = service.EstimatedEndDay,
                ModifiedDate = service.ModifiedDate,
                CreatedDate = service.CreatedDate,
                ViewCount = service.ViewCount,
                CategoryBreadcrumbs = categoryBreadcrumbs,
                Seller = seller != null ? new SellerDto
                {
                    Id = seller.Id,
                    Title = seller.Title,
                    Description = seller.Description
                } : null
            }
        }.ReturnOk();
    }

    private async Task<List<CategoryBreadcrumbDto>> GetCategoryBreadcrumbs(Guid categoryId, CancellationToken cancellationToken)
    {
        var breadcrumbs = new List<CategoryBreadcrumbDto>();
        var categories = await _categoryRepository.GetAllQuery().ToListAsync(cancellationToken);
        
        var currentCategoryId = (Guid?)categoryId;
        while (currentCategoryId.HasValue)
        {
            var category = categories.FirstOrDefault(c => c.Id == currentCategoryId.Value);
            if (category == null) break;
            
            breadcrumbs.Insert(0, new CategoryBreadcrumbDto
            {
                Id = category.Id,
                Name = category.Name
            });
            
            currentCategoryId = category.ParentId;
        }
        
        return breadcrumbs;
    }
}