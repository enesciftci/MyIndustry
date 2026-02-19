using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Dto;

public class SellerDto
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string IdentityNumber { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public int City { get; set; }
    public int District { get; set; }
    public SellerSector Sector { get; set; }
    public string AgreementUrl { get; set; }
    public SellerSubscriptionDto SellerSubscriptionDto { get; set; }
    
    // Additional fields for seller list and detail
    public int ServiceCount { get; set; }
    public string Logo { get; set; }
    public bool IsVerified { get; set; }
    public string Location { get; set; }
    public SellerInfoDto SellerInfo { get; set; }
}