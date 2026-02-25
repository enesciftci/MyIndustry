using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.ApplicationService.Handler.Category.DeleteCategoryCommand;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryCommandResult>
{
    private readonly IGenericRepository<DomainCategory> _categoryRepository;
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        IGenericRepository<DomainCategory> categoryRepository,
        IGenericRepository<Domain.Aggregate.Service> serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _serviceRepository = serviceRepository;
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

        // Check if category or any of its children has services (listings)
        var categoryIds = await GetAllCategoryIdsIncludingChildren(category, cancellationToken);
        var hasServices = await _serviceRepository
            .GetAllQuery()
            .AnyAsync(s => categoryIds.Contains(s.CategoryId), cancellationToken);

        if (hasServices)
        {
            throw new BusinessRuleException("Bu kategori veya alt kategorilerinde ilan bulunmaktadır. Kategoriyi silmek için önce ilanları silin veya başka bir kategoriye taşıyın.");
        }

        // Recursively delete all children first (deepest first), then the category itself.
        // Load all descendants by ID and delete in topological order so DB Restrict FK is satisfied.
        await DeleteCategoryAndDescendantsInOrder(category, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryCommandResult().ReturnOk("Kategori silindi.");
    }

    private async Task<List<Guid>> GetAllCategoryIdsIncludingChildren(DomainCategory category, CancellationToken cancellationToken)
    {
        var categoryIds = new List<Guid> { category.Id };

        if (category.Children?.Count > 0)
        {
            foreach (var child in category.Children)
            {
                var childWithChildren = await _categoryRepository.GetAllQuery()
                    .Include(c => c.Children)
                    .FirstOrDefaultAsync(c => c.Id == child.Id, cancellationToken);
                
                if (childWithChildren != null)
                {
                    var childIds = await GetAllCategoryIdsIncludingChildren(childWithChildren, cancellationToken);
                    categoryIds.AddRange(childIds);
                }
            }
        }

        return categoryIds;
    }

    /// <summary>
    /// Loads all descendant categories by ID, orders them so children are deleted before parents (deepest first),
    /// then deletes each. This ensures when we have Ana Kategori => Alt Kategori => Marka => Model,
    /// deleting "Marka" also deletes all "Model" rows even when Include(Children) does not populate recursively.
    /// </summary>
    private async Task DeleteCategoryAndDescendantsInOrder(DomainCategory category, CancellationToken cancellationToken)
    {
        var categoryIds = await GetAllCategoryIdsIncludingChildren(category, cancellationToken);
        if (categoryIds.Count == 0)
            return;

        var allToDelete = await _categoryRepository
            .GetAllQuery()
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var byId = allToDelete.ToDictionary(c => c.Id);
        int getDepth(DomainCategory c)
        {
            if (c.Id == category.Id) return 0;
            if (c.ParentId == null) return 0;
            return byId.TryGetValue(c.ParentId.Value, out var parent) ? 1 + getDepth(parent) : 0;
        }

        var ordered = allToDelete.OrderByDescending(c => getDepth(c)).ToList();
        foreach (var c in ordered)
            _categoryRepository.Delete(c);
    }
}
