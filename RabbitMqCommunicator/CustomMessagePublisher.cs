using MassTransit;
using MyIndustry.Container.Logging;

namespace RabbitMqCommunicator;

public class CustomMessageMessagePublisher : ICustomMessagePublisher
{
    private readonly IBus _bus;

    public CustomMessageMessagePublisher(IBus bus)
    {
        _bus = bus;
    }
    
    public async Task Publish(object message, CancellationToken cancellationToken)
    {
        var correlationId = CorrelationIdContext.Current;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            await _bus.Publish(message, context =>
            {
                context.Headers.Set(CorrelationIdConstants.HeaderName, correlationId);
            }, cancellationToken);
            return;
        }

        await _bus.Publish(message, cancellationToken);
    }
}

public interface ICustomMessagePublisher
{
    Task Publish(object message, CancellationToken cancellationToken);
}
