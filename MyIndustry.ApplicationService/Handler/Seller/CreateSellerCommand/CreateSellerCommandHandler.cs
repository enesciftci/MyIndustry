using MyIndustry.Domain.Provider;
using RabbitMqCommunicator;

namespace MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;

public sealed record CreateSellerCommandHandler: IRequestHandler<CreateSellerCommand,CreateSellerCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityProvider _securityProvider;

    public CreateSellerCommandHandler(IGenericRepository<Domain.Aggregate.Seller> sellerRepository, IUnitOfWork unitOfWork, ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _securityProvider = securityProvider;
    }

    public  async Task<CreateSellerCommandResult> Handle(CreateSellerCommand request, CancellationToken cancellationToken)
    {
        var encryptedIdentityNumber = _securityProvider.EncryptAes256(request.IdentityNumber);

        var sellerExists  = await _sellerRepository.AnyAsync(p => p.IdentityNumber == encryptedIdentityNumber, cancellationToken);

        if (sellerExists)
            throw new BusinessRuleException("Aynı VKN/TCKN ile satıcı mevcut.");

        await _sellerRepository.AddAsync(new Domain.Aggregate.Seller()
        {
            Address = request.Address,
            City = request.City,
            Description = request.Description,
            District = request.District,
            Email = request.Email,
            Sector = request.Sector,
            Title = request.Title,
            IdentityNumber = encryptedIdentityNumber,
            AgreementUrl = request.AgreementUrl,
            PhoneNumber = request.PhoneNumber
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSellerCommandResult().ReturnOk();
    }
}