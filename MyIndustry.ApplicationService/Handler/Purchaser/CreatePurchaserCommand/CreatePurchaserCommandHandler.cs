namespace MyIndustry.ApplicationService.Handler.Purchaser.CreatePurchaserCommand;

public sealed class CreatePurchaserCommandHandler : IRequestHandler<CreatePurchaserCommand,CreatePurchaserCommandResult>
{
    private readonly IGenericRepository<MyIndustry.Domain.Aggregate.Purchaser> _purchasers;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaserCommandHandler(IGenericRepository<Domain.Aggregate.Purchaser> purchasers, IUnitOfWork unitOfWork)
    {
        _purchasers = purchasers;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePurchaserCommandResult> Handle(CreatePurchaserCommand request, CancellationToken cancellationToken)
    {
        await _purchasers.AddAsync(new Domain.Aggregate.Purchaser()
        {
            IsActive = true
        },cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreatePurchaserCommandResult();
    }
}