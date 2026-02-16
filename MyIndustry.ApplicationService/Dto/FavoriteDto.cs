namespace MyIndustry.ApplicationService.Dto;

public record FavoriteDto
{
    public Guid Id { get; set; }
    public ServiceDto Service { get; set; }
    public bool IsFavorite { get; set; }
}