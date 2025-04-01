namespace MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;

public sealed class CreateCategoryCommandHandler(IGenericRepository<Domain.Aggregate.Category> categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, CreateCategoryCommandResult>
{
    public async Task<CreateCategoryCommandResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        await categoryRepository.AddAsync(new Domain.Aggregate.Category()
        {

        }, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCategoryCommandResult().ReturnOk();
    }
}