using MyIndustry.Domain.ValueObjects;

namespace CoreApiCommunicator.Request;

public class CreateSellerRequest : RequestBase
{
    public string IdentityNumber { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public int City { get; set; }
    public int District { get; set; }
    public string AgreementUrl { get; set; }
    public SellerSector Sector { get; set; }
    public Guid UserId { get; set; }
}