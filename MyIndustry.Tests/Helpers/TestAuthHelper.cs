using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyIndustry.Container.Extensions;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MyIndustry.Tests.Helpers;

public static class TestAuthHelper
{
    private const string SigningKey = "DevelopmentSigningKey-Min32CharsLong-ChangeInProduction";

    public static string CreateJwtToken(
        Guid userId,
        string email = "test@example.com",
        int userType = 0,
        string? firstName = "Test",
        string? lastName = "User")
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("uid", userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("email", email),
            new("given_name", firstName ?? ""),
            new("family_name", lastName ?? ""),
            new("type", userType.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return CreateToken(claims);
    }

    public static string CreateAdminToken(Guid? userId = null) =>
        CreateJwtToken(userId ?? Guid.NewGuid(), "admin@admin.com", 99, "Admin", "User");

    public static string CreateSellerToken(Guid? userId = null) =>
        CreateJwtToken(userId ?? Guid.NewGuid(), "seller@example.com", 2, "Seller", "User");

    public static string CreateCustomerToken(Guid? userId = null) =>
        CreateJwtToken(userId ?? Guid.NewGuid(), "customer@example.com", 0, "Customer", "User");

    private static string CreateToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: JwtExtensions.DefaultIssuer,
            audience: JwtExtensions.DefaultAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
