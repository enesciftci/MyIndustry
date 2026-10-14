using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MyIndustry.Container.Extensions;

public static class DataProtectionExtensions
{
    public const string DefaultRedisKey = "DataProtection-Keys";

    public static IServiceCollection AddMyIndustryDataProtection(
        this IServiceCollection services,
        IConnectionMultiplexer redis,
        string applicationName,
        string redisKey = DefaultRedisKey)
    {
        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(redis, redisKey)
            .SetApplicationName(applicationName);
        return services;
    }

    public static IServiceCollection AddMyIndustryDataProtectionForTesting(
        this IServiceCollection services,
        string applicationName)
    {
        services.AddDataProtection()
            .SetApplicationName(applicationName);
        return services;
    }
}
