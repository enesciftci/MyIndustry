using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetMainCategoriesQuery;

public record GetMainCategoriesQueryResult : ResponseBase
{
    public List<CategoryDto> Categories { get; set; }
}