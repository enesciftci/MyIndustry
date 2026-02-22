using MediatR;

namespace MyIndustry.ApplicationService.Handler.Category.DeleteCategoryCommand;

public class DeleteCategoryCommand : IRequest<DeleteCategoryCommandResult>
{
    public Guid Id { get; set; }
}
