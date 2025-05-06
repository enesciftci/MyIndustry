using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;

public class GetServicesByIdQueryHandler : IRequestHandler<GetServicesByIdQuery,GetServicesByIdQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;

    public GetServicesByIdQueryHandler(IGenericRepository<Domain.Aggregate.Service> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<GetServicesByIdQueryResult> Handle(GetServicesByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetById(request.Id, cancellationToken);

        if (service == null)
            throw new BusinessRuleException("Not found");
        
        return new GetServicesByIdQueryResult()
        {
            Service = new ServiceDto()
            {
                Id = service.Id,
                Price = service.Price,
                Description = service.Description,
                Title = service.Title,
                ImageUrls = service.ImageUrls,
                SellerId = service.SellerId,
                EstimatedEndDay = service.EstimatedEndDay
            }
        }.ReturnOk();
    }
}