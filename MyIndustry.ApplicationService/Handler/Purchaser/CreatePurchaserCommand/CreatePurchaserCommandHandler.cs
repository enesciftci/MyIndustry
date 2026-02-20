using MyIndustry.Domain.Aggregate;

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
        // Check if purchaser already exists
        var exists = await _purchasers.AnyAsync(p => p.Id == request.UserId, cancellationToken);
        if (exists)
        {
            return new CreatePurchaserCommandResult(); // Already exists, return success
        }

        await _purchasers.AddAsync(new Domain.Aggregate.Purchaser()
        {
            Id = request.UserId, // Link to Identity user
            IsActive = true,
            PurchaserInfo = new PurchaserInfo()
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            }
        },cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreatePurchaserCommandResult();
    }
}