using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteListQuery;

public class GetFavoriteListQueryHandler : IRequestHandler<GetFavoriteListQuery, GetFavoriteListQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Favorite> _favoriteRepository;

    public GetFavoriteListQueryHandler(IGenericRepository<Domain.Aggregate.Favorite> favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<GetFavoriteListQueryResult> Handle(GetFavoriteListQuery request, CancellationToken cancellationToken)
    {
        var favorites = await _favoriteRepository
            .GetAllQuery()
            .Where(p => p.UserId == request.UserId)
            .Include(p => p.Service)
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        var favoriteDtos = favorites
            .Where(p => p.Service != null)
            .Select(p => new FavoriteDto()
            {
                Id = p.Id,
                IsFavorite = true,
                Service = new ServiceDto()
                {
                    Id = p.Service.Id,
                    Price = new Amount(p.Service.Price).ToInt(),
                    Description = p.Service.Description,
                    Title = p.Service.Title,
                    ImageUrls = string.IsNullOrEmpty(p.Service.ImageUrls) 
                        ? Array.Empty<string>() 
                        : p.Service.ImageUrls.Split(','),
                    SellerId = p.Service.SellerId,
                    EstimatedEndDay = p.Service.EstimatedEndDay,
                    ModifiedDate = p.Service.ModifiedDate,
                    ViewCount = p.Service.ViewCount,
                    City = p.Service.City,
                    District = p.Service.District,
                    Neighborhood = p.Service.Neighborhood,
                }
            })
            .ToList();

        return new GetFavoriteListQueryResult
        {
            Favorites = favoriteDtos,
        }.ReturnOk();
    }
}