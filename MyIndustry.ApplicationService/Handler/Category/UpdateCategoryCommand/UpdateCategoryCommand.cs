using MediatR;

namespace MyIndustry.ApplicationService.Handler.Category.UpdateCategoryCommand;

public class UpdateCategoryCommand : IRequest<UpdateCategoryCommandResult>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
