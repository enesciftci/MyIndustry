using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetMainCategoriesQuery;

public class GetMainCategoriesQueryHandler(IGenericRepository<Domain.Aggregate.Category> categoryRepository)
    : IRequestHandler<GetMainCategoriesQuery, GetMainCategoriesQueryResult>
{
    public async Task<GetMainCategoriesQueryResult> Handle(GetMainCategoriesQuery request, CancellationToken cancellationToken)
    {
       
        var categories = await categoryRepository.GetAllQuery().Where(p=>p.ParentId == null).ToListAsync(cancellationToken);

         
        return new GetMainCategoriesQueryResult()
        {
            Categories = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
                // Children = BuildTree(c.Id)
            }).ToList()
        }.ReturnOk();
        // List<CategoryDto> BuildTree(Guid? parentId)
        // {
        //     return lookup[parentId]
        //         .Select(c => new CategoryDto
        //         {
        //             Id = c.Id,
        //             Name = c.Name,
        //             Children = BuildTree(c.Id)
        //         })
        //         .ToList();
        // }
        //
        // var tree = BuildTree(null);
        // return new GetMainCategoriesQueryResult()
        // {
        //     Categories = tree
        // }.ReturnOk();
    }
}
