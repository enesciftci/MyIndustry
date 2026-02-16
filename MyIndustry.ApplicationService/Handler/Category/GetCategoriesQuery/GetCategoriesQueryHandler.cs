using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

public class GetCategoriesQueryHandler(IGenericRepository<Domain.Aggregate.Category> categoryRepository)
    : IRequestHandler<GetCategoriesQuery, GetCategoriesQueryResult>
{
    public async Task<GetCategoriesQueryResult> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {

        // var categories = await categoryRepository
        //     .GetAllQuery()
        //     .Where(p => ((request.ParentId.HasValue && p.ParentId == request.ParentId) || p.ParentId == null) &&
        //                 p.IsActive)
        //     .Include(p => p.Children)
        //     // .Include(p => p.SubCategories) // Şartlı Include olmaz
        //     .Select(p => new CategoryDto()
        //     {
        //         Id = p.Id,
        //         Name = p.Name,
        //         Description = p.Description,
        //         Children = p.Children
        //             .Where(x => x.IsActive)
        //             .Select(x => new CategoryDto()
        //             {
        //                 Id = x.Id,
        //                 Name = x.Name,
        //                 Description = x.Description,
        //             }).ToList()
        //     })
        //     .ToListAsync(cancellationToken);

        var categories = await GetCategoryTreeAsync();
        var categoriDtos = categories.Select(p => new CategoryDto()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Children = p.Children
        }).ToList();
        return new GetCategoriesQueryResult()
        {
            Categories = categoriDtos
        }.ReturnOk();
    }


    public class GetCategoriesQuery2Handler(IGenericRepository<Domain.Aggregate.Category> categoryRepository)
        : IRequestHandler<GetCategoriesQuery2, GetCategoriesQueryResult>
    {
        public async Task<GetCategoriesQueryResult> Handle(GetCategoriesQuery2 request,
            CancellationToken cancellationToken)
        {
            var categories = categoryRepository.GetAllQuery().Where(p=>p.ParentId == request.ParentId && p.IsActive);

            var lookup = categories.ToLookup(c => c.ParentId);


            var categoryDtos = categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,

                })
                .ToList();
            

           
            return new GetCategoriesQueryResult()
            {
                Categories = categoryDtos
            }.ReturnOk();
        }
        
    }
    
    public async Task<List<CategoryDto>> GetCategoryTreeAsync()
    {
        var allCategories = categoryRepository.GetAllQuery();

        var lookup = allCategories.ToLookup(c => c.ParentId);

        List<CategoryDto> BuildTree(Guid? parentId)
        {
            return lookup[parentId]
                .Select(c => new CategoryDto()
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    Children = BuildTree(c.Id),
                    IsActive = c.IsActive
                })
                .ToList();
        }

        return BuildTree(null); // En üst düzey kategorilerden başla (ParentId == null)
    }
}
