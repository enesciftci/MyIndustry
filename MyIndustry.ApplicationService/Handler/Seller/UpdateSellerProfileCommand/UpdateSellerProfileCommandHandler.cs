using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Seller.UpdateSellerProfileCommand;

public sealed class UpdateSellerProfileCommandHandler : IRequestHandler<UpdateSellerProfileCommand, UpdateSellerProfileCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<SellerInfo> _sellerInfoRepository;

    public UpdateSellerProfileCommandHandler(
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IGenericRepository<SellerInfo> sellerInfoRepository)
    {
        _sellerRepository = sellerRepository;
        _sellerInfoRepository = sellerInfoRepository;
    }

    public async Task<UpdateSellerProfileCommandResult> Handle(UpdateSellerProfileCommand request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository
            .GetAllQuery()
            .Include(p => p.SellerInfo)
            .FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);

        if (seller == null)
        {
            return new UpdateSellerProfileCommandResult
            {
                Success = false,
                Message = "Satıcı bulunamadı"
            };
        }

        // Update seller fields
        seller.Title = request.Title ?? seller.Title;
        seller.Description = request.Description ?? seller.Description;
        seller.Sector = Enum.IsDefined(typeof(SellerSector), request.Sector) 
            ? (SellerSector)request.Sector 
            : seller.Sector;

        _sellerRepository.Update(seller);

        // Update SellerInfo
        if (seller.SellerInfo != null)
        {
            seller.SellerInfo.LogoUrl = request.LogoUrl ?? seller.SellerInfo.LogoUrl;
            seller.SellerInfo.TwitterUrl = request.TwitterUrl ?? seller.SellerInfo.TwitterUrl;
            seller.SellerInfo.FacebookUrl = request.FacebookUrl ?? seller.SellerInfo.FacebookUrl;
            seller.SellerInfo.InstagramUrl = request.InstagramUrl ?? seller.SellerInfo.InstagramUrl;
            seller.SellerInfo.WebSiteUrl = request.WebSiteUrl ?? seller.SellerInfo.WebSiteUrl;

            _sellerInfoRepository.Update(seller.SellerInfo);
        }

        return new UpdateSellerProfileCommandResult
        {
            Message = "Profil başarıyla güncellendi"
        }.ReturnOk();
    }
}
