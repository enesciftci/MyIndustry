using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.Provider;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerQuery;

public sealed class GetSellerQueryHandler : IRequestHandler<GetSellerQuery,GetSellerQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly ISecurityProvider _securityProvider;

    public GetSellerQueryHandler(IGenericRepository<Domain.Aggregate.Seller> sellerRepository, ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _securityProvider = securityProvider;
    }

    public async Task<GetSellerQueryResult> Handle(GetSellerQuery request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository
            .GetAllQuery()
            .Select(p => new SellerDto()
            {
                Id = p.Id,
                CreatedDate = p.CreatedDate,
                // Address = p.Address,
                // City = p.City,
                Description = p.Description,
                // District = p.District,
                Sector = p.Sector,
                Title = p.Title,
                AgreementUrl = p.AgreementUrl,
                // IdentityNumber = _securityProvider.DecryptAes256(p.IdentityNumber),
                // todo sellerinfo dan al
                // PhoneNumber = p.PhoneNumber,
                // Email = p.Email,

            })
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            

        return new GetSellerQueryResult()
        {
            Seller = seller
        }.ReturnOk();

    }
}