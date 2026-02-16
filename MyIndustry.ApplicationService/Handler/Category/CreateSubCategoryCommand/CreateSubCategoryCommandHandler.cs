using MyIndustry.Domain.Aggregate;

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
        // var category =
        //     await _categoryRepository.GetById(p => p.Id == request.SubCategory.CategoryId, cancellationToken);
        //
        // if(category == null)
        //     throw new BusinessRuleException("Category not found");
        //
        // category.SubCategories= new List<SubCategory>();
        // category.Parent.Add(new SubCategory()
        // {
        //     CategoryId = request.SubCategory.CategoryId,
        //     Description = request.SubCategory.Description,
        //     Name = request.SubCategory.Name,
        //     IsActive = true,
        // });
        //
        // _categoryRepository.Update(category);
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSubCategoryCommandResult().ReturnOk();
    }
}