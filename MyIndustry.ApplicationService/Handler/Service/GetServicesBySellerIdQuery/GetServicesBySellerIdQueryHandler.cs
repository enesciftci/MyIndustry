using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;
using System.Text.Json;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;

public class
    GetServicesBySellerIdQueryHandler : IRequestHandler<GetServicesBySellerIdQuery, GetServicesBySellerIdQueryResult>
{
    private readonly IGenericRepository<MyIndustry.Domain.Aggregate.Service> _services;

    public GetServicesBySellerIdQueryHandler(IGenericRepository<Domain.Aggregate.Service> services)
    {
        _services = services;
    }

    public async Task<GetServicesBySellerIdQueryResult> Handle(GetServicesBySellerIdQuery request,
        CancellationToken cancellationToken)
    {
        var servicesData = await _services
            .GetAllQuery()
            .Where(p => p.SellerId == request.SellerId) // Show all seller's services including inactive
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
                p.IsActive,
                p.IsApproved,
                p.CreatedDate
            })
            .ToListAsync(cancellationToken);

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
            IsActive = p.IsActive,
            IsApproved = p.IsApproved,
            CreatedDate = p.CreatedDate
        }).ToList();

        return new GetServicesBySellerIdQueryResult() {Services = services}.ReturnOk();
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