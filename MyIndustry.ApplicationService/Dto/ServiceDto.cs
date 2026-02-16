namespace MyIndustry.ApplicationService.Dto;

public class ServiceDto
{
    public Guid Id { get; set; }
    public int Price { get; set; }
    public string Description { get; set; }
    public int EstimatedEndDay { get; set; }
    public string[] ImageUrls { get; set; }
    public Guid SellerId { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public string Title { get; set; }
    public DateTime? ModifiedDate { get; set; }
}