using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.ApplicationService.Helpers;

namespace MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;

public sealed class
    CreateSubCategoryCommandHandler : IRequestHandler<CreateSubCategoryCommand, CreateSubCategoryCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubCategoryCommandHandler(IGenericRepository<Domain.Aggregate.Category> categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateSubCategoryCommandResult> Handle(CreateSubCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.SubCategory;
        if (dto == null || dto.CategoryId == Guid.Empty)
            throw new BusinessRuleException("Üst kategori seçilmedi.");

        var parentExists = await _categoryRepository.GetAllQuery()
            .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive, cancellationToken);
        if (!parentExists)
            throw new BusinessRuleException("Üst kategori bulunamadı.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessRuleException("Alt kategori adı gerekli.");

        var baseSlug = SlugHelper.GenerateSlug(dto.Name);
        var uniqueSlug = await SlugHelper.GenerateUniqueSlugAsync(
            baseSlug,
            async (slug) => await _categoryRepository.GetAllQuery()
                .AnyAsync(c => c.Slug == slug, cancellationToken)
        );

        var metaTitle = $"{dto.Name} | MyIndustry";
        var metaDescription = !string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > 160
            ? dto.Description.Substring(0, 157) + "..."
            : (dto.Description ?? $"{dto.Name} kategorisindeki ilanlar.");
        var metaKeywords = string.Join(", ", new[] { dto.Name, "sanayi", "endüstri", "myindustry" }.Distinct());

        var subCategory = new Domain.Aggregate.Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? "",
            IsActive = true,
            ParentId = dto.CategoryId,
            Slug = uniqueSlug,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords
        };

        await _categoryRepository.AddAsync(subCategory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSubCategoryCommandResult().ReturnOk();
    }
}