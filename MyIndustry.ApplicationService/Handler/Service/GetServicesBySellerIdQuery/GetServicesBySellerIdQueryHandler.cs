using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;

public class
    GetServicesBySellerIdQueryHandler : IRequestHandler<GetServicesBySellerIdQuery, GetServicesBySellerIdQueryResult>
{
    private readonly IGenericRepository<MyIndustry.Domain.Aggregate.Service> _services;

    public GetServicesBySellerIdQueryHandler(IGenericRepository<Domain.Aggregate.Service> services)
    {
        _services = services;
    }

    public async Task<GetServicesBySellerIdQueryResult> Handle(GetServicesBySellerIdQuery request,
        CancellationToken cancellationToken)
    {
        var services = await _services
            .GetAllQuery()
            .Where(p => p.SellerId == request.SellerId && p.IsActive)
            .Skip((request.Pager.Index - 1) * request.Pager.Size)
            .Take(request.Pager.Size)
            .Select(p => new ServiceDto
            {
                Id = p.Id,
                Price = p.Price,
                Description = p.Description,
                ImageUrls = p.ImageUrls,
                SellerId = p.SellerId,
                EstimatedEndDay = p.EstimatedEndDay,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        return new GetServicesBySellerIdQueryResult() {Services = services}.ReturnOk();
    }
}