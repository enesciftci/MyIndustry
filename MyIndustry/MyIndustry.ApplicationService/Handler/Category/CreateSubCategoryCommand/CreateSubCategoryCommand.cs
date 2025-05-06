using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;

public sealed record CreateSubCategoryCommand(SubCategoryDto SubCategory) : IRequest<CreateSubCategoryCommandResult>;