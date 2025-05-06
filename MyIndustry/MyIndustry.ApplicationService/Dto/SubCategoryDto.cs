namespace MyIndustry.ApplicationService.Dto;

public sealed record SubCategoryDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}