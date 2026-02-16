using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteListQuery;

public record GetFavoriteListQueryResult : ResponseBase
{
    public List<FavoriteDto> FavoriteList { get; set; }
}