using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.ApplicationService.Helpers;

namespace MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;

public sealed class CreateCategoryCommandHandler(
    IGenericRepository<Domain.Aggregate.Category> categoryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, CreateCategoryCommandResult>
{
    public async Task<CreateCategoryCommandResult> Handle(CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var currentCategory = await categoryRepository.AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (currentCategory)
            throw new BusinessRuleException("Kategori mevcut.");

        // Generate SEO slug from name
        var baseSlug = SlugHelper.GenerateSlug(request.Name);
        var uniqueSlug = await SlugHelper.GenerateUniqueSlugAsync(
            baseSlug,
            async (slug) => await categoryRepository
                .GetAllQuery()
                .AnyAsync(c => c.Slug == slug, cancellationToken)
        );

        // Generate meta title and description
        var metaTitle = $"{request.Name} | MyIndustry";
        var metaDescription = !string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 160
            ? request.Description.Substring(0, 157) + "..."
            : (request.Description ?? $"{request.Name} kategorisindeki sanayi ve endüstri ilanlarını keşfedin.");
        
        // Generate keywords from name and description
        var keywords = new List<string> { request.Name, "sanayi", "endüstri", "myindustry" };
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var descWords = request.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(5)
                .Where(w => w.Length > 3);
            keywords.AddRange(descWords);
        }
        var metaKeywords = string.Join(", ", keywords.Distinct());

        var category = new Domain.Aggregate.Category()
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            ParentId = request.ParentId,
            // SEO fields
            Slug = uniqueSlug,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords
        };

        // foreach (var subCategory in request.SubCategoryList)
        // {
        //     category.SubCategories.Add(new SubCategory()
        //     {
        //         Name = subCategory.Name,
        //         IsActive = true,
        //         Description = subCategory.Description,
        //         CategoryId = category.Id
        //     });
        // }

        await categoryRepository.AddAsync(category, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCategoryCommandResult().ReturnOk();
    }
}