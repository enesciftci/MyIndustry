using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Api.BackgroundServices;

/// <summary>
/// Süresi dolan ilanları (ExpiryDate &lt; UtcNow) pasif duruma alır. Paketteki ilan süresi (PostDurationInDays) dolunca ilan otomatik pasif olur.
/// </summary>
public class ExpiredListingsDeactivationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredListingsDeactivationService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public ExpiredListingsDeactivationService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredListingsDeactivationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredListingsDeactivationService started. Will run every {Interval}", Interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeactivateExpiredListingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deactivating expired listings");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task DeactivateExpiredListingsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MyIndustryDbContext>();
        var now = DateTime.UtcNow;

        var expired = await db.Services
            .Where(s => s.IsActive && s.ExpiryDate != null && s.ExpiryDate.Value < now)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
            return;

        foreach (var service in expired)
            service.IsActive = false;

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deactivated {Count} expired listing(s)", expired.Count);
    }
}
