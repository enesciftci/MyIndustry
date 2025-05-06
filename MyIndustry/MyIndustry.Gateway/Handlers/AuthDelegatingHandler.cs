using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.AspNetCore.Http;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MyIndustry.Gateway.Handlers;

public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("verify");
        _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var jwtValue);

        if (string.IsNullOrEmpty(jwtValue))
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        
        jwtValue = jwtValue.ToString().Replace("Bearer ", string.Empty);
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken =tokenHandler.ReadJwtToken(jwtValue);
            var userId = securityToken.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            var email = securityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            var userName = securityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            request.Headers.Add("UserId", userId);
            request.Headers.Add("Email", email);
            request.Headers.Add("UserName", userName);
        }
        catch (Exception e)
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
        
        
        return await base.SendAsync(request, cancellationToken);
    }
}