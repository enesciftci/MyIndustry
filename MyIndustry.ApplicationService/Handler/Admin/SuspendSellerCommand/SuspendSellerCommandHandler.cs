using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.ApplicationService.Handler.Admin.SuspendSellerCommand;

public class SuspendSellerCommandHandler : IRequestHandler<SuspendSellerCommand, SuspendSellerCommandResult>
{
    private readonly IGenericRepository<DomainSeller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendSellerCommandHandler(
        IGenericRepository<DomainSeller> sellerRepository,
        IUnitOfWork unitOfWork)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SuspendSellerCommandResult> Handle(SuspendSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetAllQuery()
            .FirstOrDefaultAsync(s => s.Id == request.SellerId, cancellationToken);

        if (seller == null)
        {
            return new SuspendSellerCommandResult().ReturnNotFound("Satıcı bulunamadı.");
        }

        seller.IsActive = !request.Suspend; // Suspend = inactive
        seller.ModifiedDate = DateTime.UtcNow;

        _sellerRepository.Update(seller);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = request.Suspend ? "Satıcı hesabı donduruldu." : "Satıcı hesabı aktifleştirildi.";
        return new SuspendSellerCommandResult().ReturnOk(message);
    }
}
