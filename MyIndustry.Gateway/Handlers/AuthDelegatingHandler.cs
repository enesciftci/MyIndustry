using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MyIndustry.Gateway.Handlers;

public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthDelegatingHandler(IHttpContextAccessor httpContextAccessor, TokenValidationParameters tokenValidationParameters)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenValidationParameters = tokenValidationParameters;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var jwtValue);

        if (string.IsNullOrEmpty(jwtValue))
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);

        jwtValue = jwtValue.ToString().Replace("Bearer ", string.Empty);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Token doğrulama
            var principal = tokenHandler.ValidateToken(jwtValue, _tokenValidationParameters, out var validatedToken);

            // Claimleri al
            var userId = principal.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            var email = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            var userName = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            request.Headers.Add("UserId", userId ?? string.Empty);
            request.Headers.Add("Email", email ?? string.Empty);
            request.Headers.Add("UserName", userName ?? string.Empty);
        }
        catch (SecurityTokenExpiredException)
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Token expired"
            };
        }
        catch (Exception)
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
