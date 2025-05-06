namespace MyIndustry.ApplicationService.Handler.Category.UpdateCategoryCommand;

public sealed class UpdateCategoryCommandHandler(
    IGenericRepository<Domain.Aggregate.Category> categoryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommand, UpdateCategoryCommandResult>
{
    public async Task<UpdateCategoryCommandResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        await categoryRepository.AddAsync(new Domain.Aggregate.Category()
        {
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryCommandResult().ReturnOk();
    }
}