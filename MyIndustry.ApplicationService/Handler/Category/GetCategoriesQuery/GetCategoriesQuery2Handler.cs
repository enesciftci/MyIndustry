using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

public class GetCategoriesQuery2Handler(IGenericRepository<Domain.Aggregate.Category> categoryRepository)
    : IRequestHandler<GetCategoriesQuery2, GetCategoriesQueryResult>
{
    public async Task<GetCategoriesQueryResult> Handle(GetCategoriesQuery2 request, CancellationToken cancellationToken)
    {
        // If ParentId is null, return the full tree structure
        if (request.ParentId == null)
        {
            var categories = await GetCategoryTreeAsync();
            return new GetCategoriesQueryResult()
            {
                Categories = categories
            }.ReturnOk();
        }
        
        // If ParentId is specified, return only children of that category
        var children = await categoryRepository.GetAllQuery()
            .Where(p => p.ParentId == request.ParentId && p.IsActive)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
            
        return new GetCategoriesQueryResult()
        {
            Categories = children
        }.ReturnOk();
    }
    
    private async Task<List<CategoryDto>> GetCategoryTreeAsync()
    {
        // Get all active categories
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

        return BuildTree(null); // Start from root categories (ParentId == null)
    }
}
