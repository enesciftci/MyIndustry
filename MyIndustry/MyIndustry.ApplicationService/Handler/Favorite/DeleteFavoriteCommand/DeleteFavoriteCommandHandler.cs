namespace MyIndustry.ApplicationService.Handler.Favorite.DeleteFavoriteCommand;

public class DeleteFavoriteCommandHandler : IRequestHandler<DeleteFavoriteCommand, DeleteFavoriteCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Favorite> _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFavoriteCommandHandler(IUnitOfWork unitOfWork,
        IGenericRepository<Domain.Aggregate.Favorite> favoriteRepository)
    {
        _unitOfWork = unitOfWork;
        _favoriteRepository = favoriteRepository;
    }

    public async Task<DeleteFavoriteCommandResult> Handle(DeleteFavoriteCommand request, CancellationToken cancellationToken)
    {
        var favorite = await _favoriteRepository.GetById(request.Id, cancellationToken);
        _favoriteRepository.Delete(favorite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new DeleteFavoriteCommandResult().ReturnOk();
    }
}