namespace MyIndustry.ApplicationService.Handler.Favorite.DeleteFavoriteCommand;

public record DeleteFavoriteCommand : IRequest<DeleteFavoriteCommandResult>
{
    public Guid UserId { get; set; }
    public Guid FavoriteId { get; set; }
    public Guid ServiceId { get; set; }
}