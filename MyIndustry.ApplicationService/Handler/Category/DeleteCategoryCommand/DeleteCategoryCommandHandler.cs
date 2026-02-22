using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.ApplicationService.Handler.Category.DeleteCategoryCommand;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryCommandResult>
{
    private readonly IGenericRepository<DomainCategory> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        IGenericRepository<DomainCategory> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteCategoryCommandResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAllQuery()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return new DeleteCategoryCommandResult().ReturnNotFound("Kategori bulunamadı.");
        }

        // Recursively delete all children
        await DeleteCategoryAndChildren(category, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryCommandResult().ReturnOk("Kategori silindi.");
    }

    private async Task DeleteCategoryAndChildren(DomainCategory category, CancellationToken cancellationToken)
    {
        // First delete all children recursively
        if (category.Children?.Count > 0)
        {
            foreach (var child in category.Children.ToList())
            {
                // Load children for this child
                var childWithChildren = await _categoryRepository.GetAllQuery()
                    .Include(c => c.Children)
                    .FirstOrDefaultAsync(c => c.Id == child.Id, cancellationToken);
                
                if (childWithChildren != null)
                {
                    await DeleteCategoryAndChildren(childWithChildren, cancellationToken);
                }
            }
        }

        // Then delete the category itself
        _categoryRepository.Delete(category);
    }
}
