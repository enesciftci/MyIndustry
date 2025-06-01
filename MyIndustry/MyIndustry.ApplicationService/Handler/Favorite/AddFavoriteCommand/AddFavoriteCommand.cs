namespace MyIndustry.ApplicationService.Handler.Favorite.AddFavoriteCommand;

public class AddFavoriteCommand : IRequest<AddFavoriteCommandResult>
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
}