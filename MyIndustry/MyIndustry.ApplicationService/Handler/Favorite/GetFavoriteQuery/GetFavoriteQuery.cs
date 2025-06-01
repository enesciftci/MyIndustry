namespace MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteQuery;

public record GetFavoriteQuery : IRequest<GetFavoriteQueryResult>
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
}