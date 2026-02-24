using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;
using System.Text.Json;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;

public class GetServicesByRandomlyQueryHandler : IRequestHandler<GetServicesByRandomlyQuery,GetServicesByRandomlyQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _servicesRepository;

    public GetServicesByRandomlyQueryHandler(IGenericRepository<Domain.Aggregate.Service> servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }

    public async Task<GetServicesByRandomlyQueryResult> Handle(GetServicesByRandomlyQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var servicesData = await _servicesRepository
            .GetAllQuery()
            .Where(p => p.IsActive && p.IsApproved && (p.ExpiryDate == null || p.ExpiryDate > now))
            .OrderByDescending(p => p.IsFeatured)  // Featured listings first
            .ThenByDescending(p => p.CreatedDate)  // Then by creation date
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
                p.IsFeatured
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
            IsFeatured = p.IsFeatured
        }).ToList();

        return new GetServicesByRandomlyQueryResult()
        {
            Services = services,
        }.ReturnOk();
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