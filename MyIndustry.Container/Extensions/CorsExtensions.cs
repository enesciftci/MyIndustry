using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyIndustry.Container.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddMyIndustryCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if ((environment.IsDevelopment() || environment.IsEnvironment("Testing")) && allowedOrigins.Length == 0)
        {
            allowedOrigins =
            [
                "http://localhost:3000",
                "https://localhost:3000"
            ];
        }

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing") && allowedOrigins.Length == 0)
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must be set in Production (e.g. Cors__AllowedOrigins__0=https://your-domain.com).");

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                if (allowedOrigins.Length > 0)
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                else
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            });
        });

        return services;
    }
}
