using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace MyIndustry.Container.Extensions;

public static class JwtExtensions
{
    public const string DefaultDevelopmentSigningKey = "DevelopmentSigningKey-Min32CharsLong-ChangeInProduction";
    public const string DefaultIssuer = "MyIndustry.Identity";
    public const string DefaultAudience = "myindustry";

    public static string ResolveSigningKey(IConfiguration configuration, IHostEnvironment environment)
    {
        var jwtSigningKey = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(jwtSigningKey) && !environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
            throw new InvalidOperationException("Jwt:SigningKey must be set in configuration or environment (e.g. Jwt__SigningKey).");

        return string.IsNullOrWhiteSpace(jwtSigningKey) ? DefaultDevelopmentSigningKey : jwtSigningKey;
    }

    public static string ResolveIssuer(IConfiguration configuration) =>
        configuration["Jwt:Issuer"] ?? DefaultIssuer;

    public static TokenValidationParameters CreateTokenValidationParameters(
        string signingKey,
        string issuer,
        string audience = DefaultAudience)
    {
        return new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ClockSkew = TimeSpan.Zero
        };
    }

    public static IServiceCollection AddMyIndustryJwtBearer(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Action<JwtBearerOptions>? configure = null)
    {
        var signingKey = ResolveSigningKey(configuration, environment);
        var issuer = ResolveIssuer(configuration);
        var validationParameters = CreateTokenValidationParameters(signingKey, issuer);

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Audience = DefaultAudience;
                options.RequireHttpsMetadata = !environment.IsDevelopment() && !environment.IsEnvironment("Testing");
                options.TokenValidationParameters = validationParameters;
                configure?.Invoke(options);
            });

        services.AddSingleton(validationParameters);
        return services;
    }
}
