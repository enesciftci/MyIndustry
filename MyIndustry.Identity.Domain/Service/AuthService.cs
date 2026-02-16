using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

    public AuthService(UserManager<ApplicationUser> userManager, IRedisCommunicator redisCommunicator)
    {
        _userManager = userManager;
        _redisCommunicator = redisCommunicator;
    }

    public async Task<AuthenticationModel> GetTokenAsync(string email, string password)
    {
        AuthenticationModel authenticationModel;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthenticationModel { IsAuthenticated = false, Message = $"No Accounts Registered with { email }." };

        if (await _userManager.CheckPasswordAsync(user, password))
        {
            string jwtSecurityToken = await CreateJwtToken(user);

            authenticationModel = new AuthenticationModel
            {
                IsAuthenticated = true,
                Message = jwtSecurityToken,
                Token = jwtSecurityToken,
            };
            
            await _redisCommunicator.SetCacheValueAsync($"auth:{user.Email}",authenticationModel, TimeSpan.FromHours(24));

            return authenticationModel;
        }
        else
        {
            authenticationModel = new AuthenticationModel()
            {
                IsAuthenticated = false,
                Message = $"Incorrect Credentials for user {user.Email}."
            };
        }


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

    private async Task<string> CreateJwtToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = "O'<wl]8K:1m!4g+h24R7X,HSDlv0W[b7z.`3'A$b~cPb[f('Oox|~vNz_g]&<:u"u8.ToArray();
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
                new Claim("type", user.Type.ToString()),
                new Claim("uid", user.Id)}
            .Union(userClaims)
            .Union(roleClaims);
        
       var claimsIdentity = new ClaimsIdentity();
       claimsIdentity.AddClaims(claims);
       
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.Now.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Audience = "myindustry"
            
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}

public interface IAuthService
{
    Task<AuthenticationModel> GetTokenAsync(string email, string password);
    Task<bool> RemoveTokenAsync(string id);
    Task<bool> RemoveTokenWithEmailAsync(string email);
}