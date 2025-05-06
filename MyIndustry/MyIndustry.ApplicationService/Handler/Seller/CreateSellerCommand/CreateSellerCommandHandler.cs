using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Provider;
using RabbitMqCommunicator;

namespace MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;

public sealed record CreateSellerCommandHandler : IRequestHandler<CreateSellerCommand, CreateSellerCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityProvider _securityProvider;

    public CreateSellerCommandHandler(IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IUnitOfWork unitOfWork, ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _securityProvider = securityProvider;
    }

    public async Task<CreateSellerCommandResult> Handle(CreateSellerCommand request,
        CancellationToken cancellationToken)
    {
        string encryptedIdentityNumber = null;
        if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
        {
            encryptedIdentityNumber = _securityProvider.EncryptAes256(request.IdentityNumber);
        }

        var sellerExists =
            await _sellerRepository.AnyAsync(p => p.IdentityNumber == encryptedIdentityNumber, cancellationToken);

        if (sellerExists)
            throw new BusinessRuleException("Aynı VKN/TCKN ile satıcı mevcut.");

        var seller = new Domain.Aggregate.Seller()
        {
            Id = request.UserId,
            Description = request.Description,
            Sector = request.Sector,
            Title = request.Title,
            IdentityNumber = encryptedIdentityNumber,
            AgreementUrl = request.AgreementUrl,
            IsActive = true,
            SellerInfo = new SellerInfo()
            {
                Email = request.Email, 
                PhoneNumber = request.PhoneNumber
            }
        };

        await _sellerRepository.AddAsync(seller, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSellerCommandResult().ReturnOk();
    }
}