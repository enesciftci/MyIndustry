using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.ApplicationService.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.ApplicationService.Handler.Category.UpdateCategoryCommand;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryCommandResult>
{
    private readonly IGenericRepository<DomainCategory> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        IGenericRepository<DomainCategory> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateCategoryCommandResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAllQuery()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return new UpdateCategoryCommandResult().ReturnNotFound("Kategori bulunamadı.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.ModifiedDate = DateTime.UtcNow;
        
        // Update SEO fields if name changed
        if (category.Name != request.Name || string.IsNullOrEmpty(category.Slug))
        {
            var baseSlug = SlugHelper.GenerateSlug(request.Name);
            var uniqueSlug = await SlugHelper.GenerateUniqueSlugAsync(
                baseSlug,
                async (slug) => await _categoryRepository
                    .GetAllQuery()
                    .AnyAsync(c => c.Slug == slug && c.Id != category.Id, cancellationToken)
            );
            category.Slug = uniqueSlug;
        }
        
        // Update meta fields
        category.MetaTitle = $"{request.Name} | MyIndustry";
        category.MetaDescription = !string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 160
            ? request.Description.Substring(0, 157) + "..."
            : (request.Description ?? $"{request.Name} kategorisindeki sanayi ve endüstri ilanlarını keşfedin.");
        
        // Generate keywords
        var keywords = new List<string> { request.Name, "sanayi", "endüstri", "myindustry" };
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var descWords = request.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(5)
                .Where(w => w.Length > 3);
            keywords.AddRange(descWords);
        }
        category.MetaKeywords = string.Join(", ", keywords.Distinct());

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryCommandResult().ReturnOk("Kategori güncellendi.");
    }
}
