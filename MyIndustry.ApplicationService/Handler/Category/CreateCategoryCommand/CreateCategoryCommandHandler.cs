using MyIndustry.Domain.Aggregate;

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

        var category = new Domain.Aggregate.Category()
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            ParentId = request.ParentId
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