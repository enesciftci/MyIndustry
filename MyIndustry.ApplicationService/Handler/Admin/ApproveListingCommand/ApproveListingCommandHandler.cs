using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;

public class ApproveListingCommandHandler : IRequestHandler<ApproveListingCommand, ApproveListingCommandResult>
{
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveListingCommandHandler(
        IGenericRepository<DomainService> serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApproveListingCommandResult> Handle(ApproveListingCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetAllQuery()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
        {
            return new ApproveListingCommandResult().ReturnNotFound("İlan bulunamadı.");
        }

        if (request.Approve)
        {
            service.IsApproved = true;
            service.IsActive = true;
            service.ModifiedDate = DateTime.UtcNow;
        }
        else
        {
            service.IsApproved = false;
            service.IsActive = false; // Rejected listings are deactivated
            service.ModifiedDate = DateTime.UtcNow;
            // Note: RejectionReason could be stored if we add a field for it
        }

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = request.Approve ? "İlan başarıyla onaylandı." : "İlan reddedildi.";
        return new ApproveListingCommandResult().ReturnOk(message);
    }
}
