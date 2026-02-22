using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
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

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryCommandResult().ReturnOk("Kategori güncellendi.");
    }
}
