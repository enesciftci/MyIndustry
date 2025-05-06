namespace MyIndustry.Identity.Domain.Service;

public record EmailConfirmationDto
{
    public string UserId { get; set; }
    public string Token { get; set; }
}