using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyIndustry.Identity.Domain.Aggregate;
using RedisCommunicator;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MyIndustry.Identity.Domain.Service;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRedisCommunicator _redisCommunicator;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IRedisCommunicator redisCommunicator, IConfiguration configuration)
    {
        _userManager = userManager;
        _redisCommunicator = redisCommunicator;
        _configuration = configuration;
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
        var reuseMarker = await _redisCommunicator.GetCacheValueAsync<string>($"refresh_used:{refreshToken}");
        if (reuseMarker != null)
        {
            var reusedTokenData = await _redisCommunicator.GetCacheValueAsync<RefreshTokenData>($"refresh:{refreshToken}");
            if (reusedTokenData != null)
                await InvalidateUserRefreshTokensAsync(reusedTokenData.UserId);

            return new AuthenticationModel { IsAuthenticated = false, Message = "Invalid or expired refresh token." };
        }

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

        await _redisCommunicator.SetCacheValueAsync($"refresh_used:{refreshToken}", "1", TimeSpan.FromDays(7));

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

    private Task InvalidateUserRefreshTokensAsync(string userId)
    {
        // Best-effort invalidation marker; active refresh entries expire naturally.
        return _redisCommunicator.SetCacheValueAsync($"refresh_invalidate:{userId}", DateTime.UtcNow.ToString("O"), TimeSpan.FromDays(7));
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

    /// <summary>
    /// Logout'ta kullanılmak üzere token'ı blacklist'e ekler; süre dolana kadar geçersiz sayılır.
    /// </summary>
    public async Task AddTokenToBlacklistAsync(string jti, DateTime expUtc)
    {
        if (string.IsNullOrEmpty(jti)) return;
        var ttl = expUtc - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero) return;
        await _redisCommunicator.SetCacheValueAsync("jwt_blacklist:" + jti, "1", ttl);
    }

    /// <summary>
    /// Token'ın blacklist'te olup olmadığını kontrol eder.
    /// </summary>
    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        if (string.IsNullOrEmpty(jti)) return false;
        var value = await _redisCommunicator.GetCacheValueAsync<string>("jwt_blacklist:" + jti);
        return value != null;
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
        var jwtKey = _configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured. Set it in appsettings or environment.");
        var issuer = _configuration["Jwt:Issuer"] ?? "MyIndustry.Identity";
        var key = Encoding.UTF8.GetBytes(jwtKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();
        foreach (var t in roles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, t));
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
            Issuer = issuer,
            Audience = "myindustry",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            
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
    Task AddTokenToBlacklistAsync(string jti, DateTime expUtc);
    Task<bool> IsTokenBlacklistedAsync(string jti);
}