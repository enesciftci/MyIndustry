using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;

public class GetServicesByRandomlyQueryHandler : IRequestHandler<GetServicesByRandomlyQuery,GetServicesByRandomlyQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _servicesRepository;

    public GetServicesByRandomlyQueryHandler(IGenericRepository<Domain.Aggregate.Service> servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }

    public async Task<GetServicesByRandomlyQueryResult> Handle(GetServicesByRandomlyQuery request, CancellationToken cancellationToken)
    {
        var randomService = await _servicesRepository
            .FromSqlRaw($"SELECT * FROM \"Services\" where \"IsActive\" = true ORDER BY RANDOM() LIMIT {request.Pager.Size}")
            .Select(p=>new ServiceDto
            {
                Id = p.Id,
                Price = p.Price,
                Description = p.Description,
                ImageUrls = p.ImageUrls,
                SellerId = p.SellerId,
                EstimatedEndDay = p.EstimatedEndDay
            })
            .ToListAsync(cancellationToken: cancellationToken);


        return new GetServicesByRandomlyQueryResult() {Services = randomService}.ReturnOk();
    }
}