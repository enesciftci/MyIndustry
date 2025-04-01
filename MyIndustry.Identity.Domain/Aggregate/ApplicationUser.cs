using Microsoft.AspNetCore.Identity;

namespace MyIndustry.Identity.Domain.Aggregate;

public class ApplicationUser :IdentityUser
{
    public override string UserName
    {
        get => Email; // Username olarak e-posta adresini kullanıyoruz
        set => Email = value; // E-posta adresini UserName olarak belirliyoruz
    }
}