using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.Provider;

namespace MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;

public sealed class GetSellerListQueryHandler : IRequestHandler<GetSellerListQuery,GetSellerListQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly ISecurityProvider _securityProvider;

    public GetSellerListQueryHandler(IGenericRepository<Domain.Aggregate.Seller> sellerRepository, ISecurityProvider securityProvider)
    {
        _sellerRepository = sellerRepository;
        _securityProvider = securityProvider;
    }

    public async Task<GetSellerListQueryResult> Handle(GetSellerListQuery request, CancellationToken cancellationToken)
    {
        var sellers = await _sellerRepository
            .GetAllQuery()
            .Skip((request.Pager.Index - 1) * request.Pager.Size)
            .Take(request.Pager.Size)
            .Select(p=>new SellerDto()
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
                // todo seller infodan al
                // PhoneNumber = p.PhoneNumber,
                // Email = p.Email
            })
            .ToListAsync(cancellationToken);

        return new GetSellerListQueryResult()
        {
            Sellers = sellers
        }.ReturnOk();

    }
}