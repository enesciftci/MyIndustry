using Microsoft.Extensions.Logging;

namespace MyIndustry.Container.Logging;

public static class AdminAuditLogger
{
    public static void LogAdminAction(
        ILogger logger,
        string action,
        string? actorId,
        string? targetId,
        string? correlationId,
        object? details = null)
    {
        logger.LogInformation(
            "Admin action: {AdminAction} ActorId={ActorId} TargetId={TargetId} CorrelationId={CorrelationId} {@Details}",
            action,
            actorId,
            targetId,
            correlationId,
            details);
    }
}
