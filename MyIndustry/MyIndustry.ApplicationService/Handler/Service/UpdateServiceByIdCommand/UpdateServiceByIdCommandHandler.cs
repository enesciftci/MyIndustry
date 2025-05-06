namespace MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;

public class UpdateServiceByIdCommandHandler : IRequestHandler<UpdateServiceByIdCommand, UpdateServiceByIdCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    public async Task<UpdateServiceByIdCommandResult> Handle(UpdateServiceByIdCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetById(p=>p.Id == request.ServiceDto.Id && p.SellerId == request.ServiceDto.SellerId, cancellationToken);

        if (service is not { IsActive: true })
        {
            throw new BusinessRuleException("Servis bulunamadı.");
        }
        
        service.Title = request.ServiceDto.Title;
        service.Description = request.ServiceDto.Description;
        service.ImageUrls = request.ServiceDto.ImageUrls;
        service.Price = request.ServiceDto.Price;
        service.EstimatedEndDay = request.ServiceDto.EstimatedEndDay;

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateServiceByIdCommandResult().ReturnOk();
    }
}