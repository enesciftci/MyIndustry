using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

public record GetCategoriesQueryResult : ResponseBase
{
    public List<CategoryDto> Categories { get; set; }
}