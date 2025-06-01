namespace MyIndustry.ApplicationService.Handler.Favorite.AddFavoriteCommand;

public class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, AddFavoriteCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Favorite> _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddFavoriteCommandHandler(IUnitOfWork unitOfWork,
        IGenericRepository<Domain.Aggregate.Favorite> favoriteRepository)
    {
        _unitOfWork = unitOfWork;
        _favoriteRepository = favoriteRepository;
    }

    public async Task<AddFavoriteCommandResult> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var favoriteExists =
            await _favoriteRepository.AnyAsync(p =>
                p.UserId == request.UserId &&
                p.ServiceId == request.ServiceId, cancellationToken);

        if (favoriteExists)
            return new AddFavoriteCommandResult().ReturnOk();
        
        await _favoriteRepository.AddAsync(new Domain.Aggregate.Favorite()
        {
            UserId = request.UserId,
            ServiceId = request.ServiceId,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new AddFavoriteCommandResult().ReturnOk();
    }
}