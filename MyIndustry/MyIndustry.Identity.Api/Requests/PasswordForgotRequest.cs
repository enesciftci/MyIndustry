namespace MyIndustry.Identity.Api.Requests;

public class PasswordForgotRequest
{
    public string Email { get; set; }
    public string ClientUrl { get; set; } // Frontend'ten gelen şifre sıfırlama sayfası URL'si
}