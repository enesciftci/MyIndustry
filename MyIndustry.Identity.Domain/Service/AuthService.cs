using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyIndustry.Identity.Domain.Aggregate;
using RedisCommunicator;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MyIndustry.Identity.Domain.Service;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRedisCommunicator _redisCommunicator;
    private const string JwtKey = "O'<wl]8K:1m!4g+h24R7X,HSDlv0W[b7z.`3'A$b~cPb[f('Oox|~vNz_g]&<:u";

    public AuthService(UserManager<ApplicationUser> userManager, IRedisCommunicator redisCommunicator)
    {
        _userManager = userManager;
        _redisCommunicator = redisCommunicator;
    }

    public async Task<AuthenticationModel> GetTokenAsync(string email, string password)
    {
        AuthenticationModel authenticationModel;
        const string invalidCredentialsMessage = "Geçersiz kullanıcı adı veya şifre.";
        
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthenticationModel { IsAuthenticated = false, Message = invalidCredentialsMessage };

        if (await _userManager.CheckPasswordAsync(user, password))
        {
            string jwtSecurityToken = await CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            authenticationModel = new AuthenticationModel
            {
                IsAuthenticated = true,
                Message = jwtSecurityToken,
                Token = jwtSecurityToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry
            };
            
            // Store refresh token in Redis with user email as key
            await _redisCommunicator.SetCacheValueAsync($"refresh:{refreshToken}", new RefreshTokenData
            {
                UserId = user.Id,
                Email = user.Email,
                Expiry = refreshTokenExpiry
            }, TimeSpan.FromDays(7));
            
            await _redisCommunicator.SetCacheValueAsync($"auth:{user.Email}", authenticationModel, TimeSpan.FromHours(24));

            return authenticationModel;
        }
        else
        {
            authenticationModel = new AuthenticationModel()
            {
                IsAuthenticated = false,
                Message = invalidCredentialsMessage
            };
        }

        return authenticationModel;
    }

    public async Task<AuthenticationModel> RefreshTokenAsync(string refreshToken)
    {
        // Get refresh token data from Redis
        var tokenData = await _redisCommunicator.GetCacheValueAsync<RefreshTokenData>($"refresh:{refreshToken}");
        
        if (tokenData == null || tokenData.Expiry < DateTime.UtcNow)
        {
            return new AuthenticationModel { IsAuthenticated = false, Message = "Invalid or expired refresh token." };
        }

        var user = await _userManager.FindByIdAsync(tokenData.UserId);
        if (user == null)
        {
            return new AuthenticationModel { IsAuthenticated = false, Message = "User not found." };
        }

        // Delete old refresh token
        _redisCommunicator.DeleteValue($"refresh:{refreshToken}");

        // Generate new tokens
        var newAccessToken = await CreateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var authenticationModel = new AuthenticationModel
        {
            IsAuthenticated = true,
            Message = newAccessToken,
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiry = newRefreshTokenExpiry,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email
            }
        };

        // Store new refresh token
        await _redisCommunicator.SetCacheValueAsync($"refresh:{newRefreshToken}", new RefreshTokenData
        {
            UserId = user.Id,
            Email = user.Email,
            Expiry = newRefreshTokenExpiry
        }, TimeSpan.FromDays(7));

        await _redisCommunicator.SetCacheValueAsync($"auth:{user.Email}", authenticationModel, TimeSpan.FromHours(24));

        return authenticationModel;
    }

    public async Task<bool> RemoveTokenAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user != null && _redisCommunicator.DeleteValue($"auth:{user.Email}");
    }
    
    public async Task<bool> RemoveTokenWithEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null && _redisCommunicator.DeleteValue($"auth:{user.Email}");
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> CreateJwtToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(JwtKey);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();
        foreach (var t in roles)
        {
            roleClaims.Add(new Claim("roles", t));
        }
        var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("type", ((int)user.Type).ToString()),
                new Claim("uid", user.Id)}
            .Union(userClaims)
            .Union(roleClaims);
        
       var claimsIdentity = new ClaimsIdentity();
       claimsIdentity.AddClaims(claims);
       
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Audience = "myindustry"
            
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class RefreshTokenData
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public DateTime Expiry { get; set; }
}

public interface IAuthService
{
    Task<AuthenticationModel> GetTokenAsync(string email, string password);
    Task<AuthenticationModel> RefreshTokenAsync(string refreshToken);
    Task<bool> RemoveTokenAsync(string id);
    Task<bool> RemoveTokenWithEmailAsync(string email);
}