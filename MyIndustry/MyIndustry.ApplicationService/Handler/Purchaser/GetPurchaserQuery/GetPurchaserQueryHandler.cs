using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Purchaser.GetPurchaserQuery;

public sealed class GetPurchaserQueryHandler : IRequestHandler<GetPurchaserQuery, GetPurchaserQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Purchaser> _purchaserRepository;

    public GetPurchaserQueryHandler(IGenericRepository<Domain.Aggregate.Purchaser> purchaserRepository)
    {
        _purchaserRepository = purchaserRepository;
    }

    public async Task<GetPurchaserQueryResult> Handle(GetPurchaserQuery request, CancellationToken cancellationToken)
    {
        var purchaser =
            await _purchaserRepository.GetById(p => p.Id == request.PurchaserId && p.IsActive, cancellationToken);

        if (purchaser is null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        return new GetPurchaserQueryResult()
        {
            PurchaserDto = new PurchaserDto()
            {
                Id = purchaser.Id,
                PurchaserInfoDto = new PurchaserInfoDto()
                {
                    Email = purchaser.PurchaserInfo.Email,
                    PhoneNumber = purchaser.PurchaserInfo.PhoneNumber,
                }
            }
        };
    }
}