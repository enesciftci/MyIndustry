using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

public class GetCategoriesQueryHandler(IGenericRepository<Domain.Aggregate.Category> categoryRepository)
    : IRequestHandler<GetCategoriesQuery, GetCategoriesQueryResult>
{
    public async Task<GetCategoriesQueryResult> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories =
            await categoryRepository
                .GetAllQuery()
                .Select(p => new CategoryDto())
                .ToListAsync(cancellationToken);

        return new GetCategoriesQueryResult()
        {
            Categories = categories
        }.ReturnOk();
    }
}