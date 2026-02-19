using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;
using System.Text.Json;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Admin.GetAdminListingsQuery;

public class GetAdminListingsQueryHandler : IRequestHandler<GetAdminListingsQuery, GetAdminListingsQueryResult>
{
    private readonly IGenericRepository<DomainService> _serviceRepository;

    public GetAdminListingsQueryHandler(IGenericRepository<DomainService> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<GetAdminListingsQueryResult> Handle(GetAdminListingsQuery request, CancellationToken cancellationToken)
    {
        var query = _serviceRepository.GetAllQuery()
            .Include(s => s.Seller)
            .Include(s => s.Category)
            .AsQueryable();

        // Filter by status
        switch (request.Status?.ToLower())
        {
            case "pending":
                query = query.Where(s => !s.IsApproved && s.IsActive);
                break;
            case "approved":
                query = query.Where(s => s.IsApproved && s.IsActive);
                break;
            case "rejected":
                query = query.Where(s => !s.IsActive);
                break;
            // "all" or null = no filter
        }

        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(s => 
                s.Title.ToLower().Contains(searchLower) ||
                s.Description.ToLower().Contains(searchLower) ||
                s.Seller.Title.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.Size);

        var listings = await query
            .OrderByDescending(s => s.CreatedDate)
            .Skip((request.Index - 1) * request.Size)
            .Take(request.Size)
            .Select(s => new AdminListingDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Price = s.Price,
                ImageUrls = ParseImageUrls(s.ImageUrls),
                IsApproved = s.IsApproved,
                IsActive = s.IsActive,
                ViewCount = s.ViewCount,
                CreatedDate = s.CreatedDate,
                ModifiedDate = s.ModifiedDate,
                SellerId = s.SellerId,
                SellerName = s.Seller.Title,
                SellerEmail = s.Seller.SellerInfo != null ? s.Seller.SellerInfo.Email : null,
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : null,
                Status = !s.IsActive ? "rejected" : (s.IsApproved ? "approved" : "pending")
            })
            .ToListAsync(cancellationToken);

        return new GetAdminListingsQueryResult
        {
            Listings = listings,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = request.Index
        }.ReturnOk();
    }

    private static string[] ParseImageUrls(string? imageUrls)
    {
        if (string.IsNullOrWhiteSpace(imageUrls))
            return Array.Empty<string>();
        
        try
        {
            return JsonSerializer.Deserialize<string[]>(imageUrls) ?? Array.Empty<string>();
        }
        catch
        {
            return imageUrls.StartsWith("http") ? new[] { imageUrls } : Array.Empty<string>();
        }
    }
}
