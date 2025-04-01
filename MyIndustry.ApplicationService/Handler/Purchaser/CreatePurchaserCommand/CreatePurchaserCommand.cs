namespace MyIndustry.ApplicationService.Handler.Purchaser.CreatePurchaserCommand;

public record CreatePurchaserCommand : IRequest<CreatePurchaserCommandResult>
{
    public Guid Id { get; set; }
}