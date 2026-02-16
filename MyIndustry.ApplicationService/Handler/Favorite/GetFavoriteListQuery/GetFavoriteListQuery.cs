namespace MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteListQuery;

public record GetFavoriteListQuery : IRequest<GetFavoriteListQueryResult>
{
    public Guid UserId { get; set; }
}