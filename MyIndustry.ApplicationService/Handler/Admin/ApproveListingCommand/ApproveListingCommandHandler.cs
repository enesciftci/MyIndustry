using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;

public class ApproveListingCommandHandler : IRequestHandler<ApproveListingCommand, ApproveListingCommandResult>
{
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveListingCommandHandler(
        IGenericRepository<DomainService> serviceRepository,
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApproveListingCommandResult> Handle(ApproveListingCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetAllQuery()
            .Include(s => s.Seller)
                .ThenInclude(s => s.SellerSubscriptions)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
        {
            return new ApproveListingCommandResult().ReturnNotFound("İlan bulunamadı.");
        }

        var activeSubscription = service.Seller?.SellerSubscriptions?.FirstOrDefault(ss => ss.IsActive);

        if (request.Approve)
        {
            service.IsApproved = true;
            service.IsActive = true;
            service.ModifiedDate = DateTime.UtcNow;
        }
        else
        {
            if (activeSubscription != null)
            {
                activeSubscription.RemainingPostQuota++;
                if (service.IsFeatured)
                    activeSubscription.RemainingFeaturedQuota++;
                _sellerRepository.Update(service.Seller);
            }
            
            service.IsApproved = false;
            service.IsActive = false; // Rejected listings are deactivated
            service.RejectionReasonType = request.RejectionReasonType;
            service.RejectionReasonDescription = request.RejectionReasonDescription;
            service.ModifiedDate = DateTime.UtcNow;
        }

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = request.Approve ? "İlan başarıyla onaylandı." : "İlan reddedildi ve ilan hakkı geri verildi.";
        return new ApproveListingCommandResult().ReturnOk(message);
    }
}
