using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Admin.SuspendListingCommand;

public class SuspendListingCommandHandler : IRequestHandler<SuspendListingCommand, SuspendListingCommandResult>
{
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendListingCommandHandler(
        IGenericRepository<DomainService> serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SuspendListingCommandResult> Handle(SuspendListingCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetAllQuery()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
        {
            return new SuspendListingCommandResult().ReturnNotFound("İlan bulunamadı.");
        }

        service.IsActive = !request.Suspend; // Suspend = inactive
        service.ModifiedDate = DateTime.UtcNow;

        if (request.Suspend)
        {
            service.SuspensionReasonType = request.SuspensionReasonType;
            service.SuspensionReasonDescription = request.SuspensionReasonDescription;
        }
        else
        {
            // Clear suspension reasons when unsuspending
            service.SuspensionReasonType = null;
            service.SuspensionReasonDescription = null;
        }

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = request.Suspend ? "İlan donduruldu." : "İlan aktifleştirildi.";
        return new SuspendListingCommandResult().ReturnOk(message);
    }
}
