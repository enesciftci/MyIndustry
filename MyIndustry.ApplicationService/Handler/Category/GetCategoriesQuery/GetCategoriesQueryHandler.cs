using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

public class GetCategoriesQueryHandler(IGenericRepository<Domain.Aggregate.Category> categoryRepository)
    : IRequestHandler<GetCategoriesQuery, GetCategoriesQueryResult>
{
    public async Task<GetCategoriesQueryResult> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await GetCategoryTreeAsync();
        
        return new GetCategoriesQueryResult()
        {
            Categories = categories
        }.ReturnOk();
    }
    
    public async Task<List<CategoryDto>> GetCategoryTreeAsync()
    {
        // Sadece aktif kategorileri al
        var allCategories = await categoryRepository.GetAllQuery()
            .Where(c => c.IsActive)
            .ToListAsync();

        var lookup = allCategories.ToLookup(c => c.ParentId);

        List<CategoryDto> BuildTree(Guid? parentId)
        {
            return lookup[parentId]
                .Select(c => new CategoryDto()
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    Description = c.Description,
                    Children = BuildTree(c.Id),
                    IsActive = c.IsActive
                })
                .ToList();
        }

        return BuildTree(null); // En üst düzey kategorilerden başla (ParentId == null)
    }
}
