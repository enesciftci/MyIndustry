using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteQuery;

public class GetFavoriteQueryHandler : IRequestHandler<GetFavoriteQuery, GetFavoriteQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Favorite> _favoriteRepository;

    public GetFavoriteQueryHandler(IGenericRepository<Domain.Aggregate.Favorite> favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<GetFavoriteQueryResult> Handle(GetFavoriteQuery query, CancellationToken cancellationToken)
    {
        var favorite = await _favoriteRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(p => p.UserId == query.UserId && p.ServiceId == query.ServiceId, cancellationToken);

        if (favorite == null)
            return new GetFavoriteQueryResult().ReturnOk();
        
        return new GetFavoriteQueryResult
        {
            FavoriteDto = new FavoriteDto()
            {
                Id = favorite.Id,
                IsFavorite = true,
            }
        }.ReturnOk();
    }
}