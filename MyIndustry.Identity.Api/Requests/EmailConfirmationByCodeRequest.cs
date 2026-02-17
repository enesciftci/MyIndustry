namespace MyIndustry.Identity.Api.Requests;

public class EmailConfirmationByCodeRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
}
