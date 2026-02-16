using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteQuery;

public record GetFavoriteQueryResult : ResponseBase
{
    public FavoriteDto FavoriteDto { get; set; }
}