namespace MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;

public record CreateServiceCommand : IRequest<CreateServiceCommandResult>
{
    public int Price { get; set; }
    public string Description { get; set; }
    public int EstimatedEndDay { get; set; }
    public string ImageUrls { get; set; }
    // public Guid SubCategoryId { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; }
}