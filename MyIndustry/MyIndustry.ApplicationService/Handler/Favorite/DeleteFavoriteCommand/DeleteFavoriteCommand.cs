namespace MyIndustry.ApplicationService.Handler.Favorite.DeleteFavoriteCommand;

public record DeleteFavoriteCommand : IRequest<DeleteFavoriteCommandResult>
{
    public Guid Id { get; set; }
}