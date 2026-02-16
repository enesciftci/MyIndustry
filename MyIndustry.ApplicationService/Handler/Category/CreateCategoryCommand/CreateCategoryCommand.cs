using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;

public record CreateCategoryCommand : IRequest<CreateCategoryCommandResult>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid? ParentId { get; set; }
}