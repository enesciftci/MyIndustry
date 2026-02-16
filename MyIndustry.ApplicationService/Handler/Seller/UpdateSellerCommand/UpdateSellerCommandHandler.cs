using MyIndustry.Domain.Provider;

namespace MyIndustry.ApplicationService.Handler.Seller.UpdateSellerCommand;

public sealed record UpdateSellerCommandHandler: IRequestHandler<UpdateSellerCommand,UpdateSellerCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityProvider _securityProvider;

    public UpdateSellerCommandHandler(IGenericRepository<Domain.Aggregate.Seller> sellerRepository, IUnitOfWork unitOfWork, ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _securityProvider = securityProvider;
    }

    public  async Task<UpdateSellerCommandResult> Handle(UpdateSellerCommand request, CancellationToken cancellationToken)
    {
        var encryptedIdentityNumber = _securityProvider.EncryptAes256(request.IdentityNumber);

        var seller  = await _sellerRepository.GetSingleOrDefault(request.Id, cancellationToken);

        if (seller == null)
            throw new BusinessRuleException("Satıcı bulunamadı.");

        // seller.Address = request.Address;
        // seller.City = request.City;
        seller.Description = request.Description;
        // seller.District = request.District;
        seller.Sector = request.Sector;
        seller.Title = request.Title;
        seller.IdentityNumber = encryptedIdentityNumber;
        seller.AgreementUrl = request.AgreementUrl;

        seller.SellerInfo.Email = request.Email;
        seller.SellerInfo.PhoneNumber = request.PhoneNumber;
        _sellerRepository.Update(seller);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateSellerCommandResult().ReturnOk();
    }
}