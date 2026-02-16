using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;

public class GetServicesBySearchTermQueryHandler : IRequestHandler<GetServicesBySearchTermQuery,GetServicesBySearchTermQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _servicesRepository;

    public GetServicesBySearchTermQueryHandler(IGenericRepository<Domain.Aggregate.Service> servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }

    public async Task<GetServicesBySearchTermQueryResult> Handle(GetServicesBySearchTermQuery request, CancellationToken cancellationToken)
    {
        var searchedServices = await _servicesRepository
            .GetAllQuery()
            .Where(p => 
                (p.Title.Contains(request.Query) || p.Description.Contains(request.Query)) && 
                p.IsActive)
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


        return new GetServicesBySearchTermQueryResult() {Services = searchedServices}.ReturnOk();
    }
}