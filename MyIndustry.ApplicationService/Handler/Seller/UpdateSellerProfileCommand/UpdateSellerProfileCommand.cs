using MediatR;

namespace MyIndustry.ApplicationService.Handler.Seller.UpdateSellerProfileCommand;

public sealed record UpdateSellerProfileCommand : IRequest<UpdateSellerProfileCommandResult>
{
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Sector { get; set; }
    
    // SellerInfo
    public string LogoUrl { get; set; }
    public string TwitterUrl { get; set; }
    public string FacebookUrl { get; set; }
    public string InstagramUrl { get; set; }
    public string WebSiteUrl { get; set; }
}
