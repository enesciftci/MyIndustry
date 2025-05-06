using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

public class GetCategoriesQueryHandler(IGenericRepository<Domain.Aggregate.Category> categoryRepository)
    : IRequestHandler<GetCategoriesQuery, GetCategoriesQueryResult>
{
    public async Task<GetCategoriesQueryResult> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository
            .GetAllQuery()
            .Where(p => p.IsActive)
            .Include(p => p.SubCategories) // Şartlı Include olmaz
            .Select(p => new CategoryDto()
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SubCategories = p.SubCategories
                    .Where(x => x.IsActive)
                    .Select(x => new SubCategoryDto()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                    }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new GetCategoriesQueryResult()
        {
            Categories = categories
        }.ReturnOk();
    }
}