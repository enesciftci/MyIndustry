using MassTransit;
using MyIndustry.Container.Logging;
using Serilog.Context;

namespace MyIndustry.Container.MassTransit;

public class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.Headers.Get<string>(CorrelationIdConstants.HeaderName)
            ?? context.CorrelationId?.ToString()
            ?? Guid.NewGuid().ToString();

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        using (CorrelationIdContext.BeginScope(correlationId))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("correlationIdConsume");
}
