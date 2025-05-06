using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public class GetServicesByFilterQueryHandler : IRequestHandler<GetServicesByFilterQuery, GetServicesByFilterQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;

    public GetServicesByFilterQueryHandler(IGenericRepository<Domain.Aggregate.Service> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<GetServicesByFilterQueryResult> Handle(GetServicesByFilterQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository
            .GetAllQuery()
            .Where(p =>
                p.CategoryId == request.CategoryId &&
                (request.SubCategoryId.HasValue && request.SubCategoryId.Value == p.SubCategoryId) &&
                p.IsActive)
            .Select(p=>new ServiceDto()
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageUrls = p.ImageUrls,
                Price = p.Price,
                SellerId = p.SellerId
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new GetServicesByFilterQueryResult() { Services = services }.ReturnOk();
    }
}