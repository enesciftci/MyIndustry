using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;

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
            .GetAllQuery()
            .Where(p => p.IsActive)
            .Skip((request.Pager.Index - 1) * request.Pager.Size)
            .Take(request.Pager.Size)
            .Select(p => new ServiceDto
            {
                Id = p.Id,
                Price = new Amount(p.Price).ToInt(),
                Title = p.Title,
                Description = p.Description,
                ImageUrls = new List<string>(){p.ImageUrls}.ToArray(),
                SellerId = p.SellerId,
                EstimatedEndDay = p.EstimatedEndDay
            })
            .ToListAsync(cancellationToken: cancellationToken);


        return new GetServicesByRandomlyQueryResult()
        {
            Services = randomService,
            // Pager = new Pager()
        }.ReturnOk();
    }
}