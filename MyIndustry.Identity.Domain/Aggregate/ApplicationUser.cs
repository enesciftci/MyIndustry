using Microsoft.AspNetCore.Identity;
using MyIndustry.Identity.Domain.Aggregate.ValueObjects;

namespace MyIndustry.Identity.Domain.Aggregate;

public class ApplicationUser :IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public override string UserName
    {
        get => Email; // Username olarak e-posta adresini kullanıyoruz
        set => Email = value; // E-posta adresini UserName olarak belirliyoruz
    }

    public UserType Type { get; set; }
    
    /// <summary>
    /// Kullanıcı hesabı dondurulmuş mu?
    /// </summary>
    public bool IsSuspended { get; set; } = false;
    
    /// <summary>
    /// Dondurulma sebebi
    /// </summary>
    public string? SuspensionReason { get; set; }
}