namespace MyIndustry.Identity.Domain.Aggregate.ValueObjects;

public enum UserType : byte
{
    User = 0,      // Normal kullanıcı
    Purchaser = 1, // Alıcı (deprecated, User kullanılacak)
    Seller = 2,    // Satıcı
    Admin = 99     // Admin
}