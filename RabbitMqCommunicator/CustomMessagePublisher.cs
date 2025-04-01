using MassTransit;
using MassTransit.RabbitMqTransport;
using MyIndustry.Queue.Message;

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
        await _bus.Publish(message, cancellationToken);
    }
}

public interface ICustomMessagePublisher
{
    Task Publish(object message, CancellationToken cancellationToken);
}