using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;
using System.Text.Json;

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
        var query = _serviceRepository.GetAllQuery()
            .Where(p => p.IsApproved && p.IsActive);

        // Search by service title, description, or seller/company name
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(searchLower) || 
                p.Description.ToLower().Contains(searchLower) ||
                p.Seller.Title.ToLower().Contains(searchLower));
        }

        // Filter by city/district
        if (request.CityId.HasValue)
        {
            query = query.Where(p => p.Seller.Addresses.Any(x => x.City == request.CityId.Value));

            if (request.DistrictId.HasValue)
            {
                query = query.Where(p => p.Seller.Addresses.Any(x => x.District == request.DistrictId.Value));
            }
        }

        // Filter by category (if provided)
        if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
        {
            var allCategories = await _categoryRepository.GetAllQuery().ToListAsync(cancellationToken);
            var categoryIds = GetAllCategoryIds(request.CategoryId.Value);
            query = query.Where(p => categoryIds.Contains(p.CategoryId));
            
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

        var servicesData = await query
            .Select(p => new
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageUrls = p.ImageUrls,
                Price = p.Price,
                SellerId = p.SellerId,
                ViewCount = p.ViewCount,
                EstimatedEndDay = p.EstimatedEndDay
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var services = servicesData.Select(p => new ServiceDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            ImageUrls = ParseImageUrls(p.ImageUrls),
            Price = new Amount(p.Price).ToInt(),
            SellerId = p.SellerId,
            ViewCount = p.ViewCount,
            EstimatedEndDay = p.EstimatedEndDay
        }).ToList();
        
        return new GetServicesByFilterQueryResult() { Services = services }.ReturnOk();
    }

    private static string[] ParseImageUrls(string? imageUrls)
    {
        if (string.IsNullOrWhiteSpace(imageUrls))
            return Array.Empty<string>();
        
        try
        {
            var parsed = JsonSerializer.Deserialize<string[]>(imageUrls);
            return parsed ?? Array.Empty<string>();
        }
        catch
        {
            return imageUrls.StartsWith("http") ? new[] { imageUrls } : Array.Empty<string>();
        }
    }
}