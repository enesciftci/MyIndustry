using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public class GetServicesByFilterQueryHandler : IRequestHandler<GetServicesByFilterQuery, GetServicesByFilterQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Category> _categoryRepository;

    public GetServicesByFilterQueryHandler(IGenericRepository<Domain.Aggregate.Service> serviceRepository, IGenericRepository<Domain.Aggregate.Category> categoryRepository)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<GetServicesByFilterQueryResult> Handle(GetServicesByFilterQuery request, CancellationToken cancellationToken)
    {
        var query = _serviceRepository.GetAllQuery();

        if (request.CityId.HasValue)
        {
            query = query.Where(p => p.Seller.Addresses.Any(x => x.City == request.CityId.Value));

            if (request.DistrictId.HasValue)
            {
                query = query.Where(p => p.Seller.Addresses.Any(x => x.District == request.DistrictId.Value));
            }
        }

        var allCategories = await _categoryRepository.GetAllQuery().ToListAsync(cancellationToken);

        var categoryIds = GetAllCategoryIds(request.CategoryId);

        
        var servicesData = await query
            .Where(p =>
                categoryIds.Contains(p.CategoryId) &&
                p.IsApproved &&
                p.IsActive)
            .Select(p => new
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageUrls = p.ImageUrls ,
                Price = p.Price,
                SellerId = p.SellerId
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var services = servicesData.Select(p => new ServiceDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            ImageUrls = string.IsNullOrEmpty(p.ImageUrls) 
                ? [] 
                : p.ImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries),
            Price = new Amount(p.Price).ToInt(),
            SellerId = p.SellerId
        }).ToList();
        return new GetServicesByFilterQueryResult() { Services = services }.ReturnOk();

        List<Guid> GetAllCategoryIds(Guid parentId)
        {
            var children = allCategories.Where(c => c.ParentId == parentId).ToList();
            var ids = new List<Guid> { parentId };

            foreach (var child in children)
            {
                ids.AddRange(GetAllCategoryIds(child.Id));
            }

            return ids;
        }
    }
}