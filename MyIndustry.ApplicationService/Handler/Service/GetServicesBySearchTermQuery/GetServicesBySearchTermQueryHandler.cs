using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;
using System.Text.Json;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;

public class GetServicesBySearchTermQueryHandler : IRequestHandler<GetServicesBySearchTermQuery,GetServicesBySearchTermQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _servicesRepository;

    public GetServicesBySearchTermQueryHandler(IGenericRepository<Domain.Aggregate.Service> servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }

    public async Task<GetServicesBySearchTermQueryResult> Handle(GetServicesBySearchTermQuery request, CancellationToken cancellationToken)
    {
        var searchTerm = request.Query.ToLower();
        
        // Base query with search criteria
        var baseQuery = _servicesRepository
            .GetAllQuery()
            .Where(p => 
                p.IsActive && p.IsApproved &&
                (p.Title.ToLower().Contains(searchTerm) || 
                 p.Description.ToLower().Contains(searchTerm) ||
                 (p.City != null && p.City.ToLower().Contains(searchTerm)) ||
                 (p.District != null && p.District.ToLower().Contains(searchTerm)) ||
                 (p.Neighborhood != null && p.Neighborhood.ToLower().Contains(searchTerm))));
        
        // Get total count
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        
        var servicesData = await baseQuery
            .OrderByDescending(p => p.CreatedDate)
            .Skip((request.Pager.Index - 1) * request.Pager.Size)
            .Take(request.Pager.Size)
            .Select(p => new
            {
                p.Id,
                p.Price,
                p.Title,
                p.Description,
                p.ImageUrls,
                p.SellerId,
                p.EstimatedEndDay,
                p.ViewCount,
                p.City,
                p.District,
                p.Neighborhood,
                p.Condition,
                p.ListingType,
                p.CreatedDate
            })
            .ToListAsync(cancellationToken: cancellationToken);

        var services = servicesData.Select(p => new ServiceDto
        {
            Id = p.Id,
            Price = new Amount(p.Price).ToInt(),
            Title = p.Title,
            Description = p.Description,
            ImageUrls = ParseImageUrls(p.ImageUrls),
            SellerId = p.SellerId,
            EstimatedEndDay = p.EstimatedEndDay,
            ViewCount = p.ViewCount,
            City = p.City,
            District = p.District,
            Neighborhood = p.Neighborhood,
            Condition = (int)p.Condition,
            ListingType = (int)p.ListingType,
            CreatedDate = p.CreatedDate
        }).ToList();

        return new GetServicesBySearchTermQueryResult() { Services = services, TotalCount = totalCount }.ReturnOk();
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