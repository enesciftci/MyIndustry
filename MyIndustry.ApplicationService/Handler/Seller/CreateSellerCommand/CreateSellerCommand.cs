using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;

public record CreateSellerCommand : IRequest<CreateSellerCommandResult>
{
    public Guid UserId { get; set; }
    public string IdentityNumber { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    // todo add address dto
    public string Address { get; set; }
    public int City { get; set; }
    public int District { get; set; }
    public string AgreementUrl { get; set; }
    public SellerSector Sector { get; set; }
}