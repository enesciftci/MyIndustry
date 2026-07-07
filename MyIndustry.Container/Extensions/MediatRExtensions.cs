using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyIndustry.Container.Logging;
using MyIndustry.Container.MediatR;

namespace MyIndustry.Container.Extensions;

public static class MediatRExtensions
{
    public static IServiceCollection AddMyIndustryMediatRLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MediatRLoggingOptions>(
            configuration.GetSection(MediatRLoggingOptions.SectionName));

        return services;
    }

    public static MediatRServiceConfiguration AddMyIndustryLoggingBehavior(
        this MediatRServiceConfiguration configuration)
    {
        configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        return configuration;
    }
}
