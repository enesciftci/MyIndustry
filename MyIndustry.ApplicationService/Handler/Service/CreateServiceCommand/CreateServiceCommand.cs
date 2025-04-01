namespace MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;

public record CreateServiceCommand : IRequest<CreateServiceCommandResult>
{
    public decimal Price { get; set; }
    public string Description { get; set; }
    public int EstimatedEndDay { get; set; }
    public string ImageUrls { get; set; }
    public Guid SellerId { get; set; }
}