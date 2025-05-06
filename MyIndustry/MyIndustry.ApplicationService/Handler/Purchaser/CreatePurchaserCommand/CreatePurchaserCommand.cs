namespace MyIndustry.ApplicationService.Handler.Purchaser.CreatePurchaserCommand;

public record CreatePurchaserCommand : IRequest<CreatePurchaserCommandResult>
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}