namespace MyIndustry.ApplicationService.Dto;

public sealed record CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<SubCategoryDto> SubCategories { get; set; }
}