using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.ValueObjects;

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
        var query = _serviceRepository.GetAllQuery();

        if (request.SubCategoryId.HasValue)
        {
            query = query.Where(p => p.SubCategoryId == request.SubCategoryId.Value);
        }

        if (request.CityId.HasValue)
        {
            query = query.Where(p => p.Seller.Addresses.Any(x => x.City == request.CityId.Value));

            if (request.DistrictId.HasValue)
            {
                query = query.Where(p => p.Seller.Addresses.Any(x => x.District == request.DistrictId.Value));
            }
        }

        var services = await query
            .Where(p =>
                p.CategoryId == request.CategoryId &&
                p.IsApproved &&
                p.IsActive)
            .Select(p => new ServiceDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageUrls = new List<string>(){p.ImageUrls}.ToArray(),
                Price = new Amount(p.Price).ToInt(),
                SellerId = p.SellerId
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new GetServicesByFilterQueryResult() { Services = services }.ReturnOk();
    }
}