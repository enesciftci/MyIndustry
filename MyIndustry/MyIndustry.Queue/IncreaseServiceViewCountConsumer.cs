using CoreApiCommunicator;
using CoreApiCommunicator.Request;
using CoreApiCommunicator.Response;
using MassTransit;
using MyIndustry.Queue.Message;

namespace MyIndustry.Queue;

public class IncreaseServiceViewCountConsumer : IConsumer<IncreaseServiceViewCountMessage>
{
    private readonly ICoreApiCommunicator<IncreaseServiceViewCountRequest, ResponseBase> _coreApiCommunicator;

    public IncreaseServiceViewCountConsumer(ICoreApiCommunicator<IncreaseServiceViewCountRequest, ResponseBase> coreApiCommunicator)
    {
        _coreApiCommunicator = coreApiCommunicator;
    }

    public async Task Consume(ConsumeContext<IncreaseServiceViewCountMessage> context)
    {
        var message = context.Message;
        var request = new IncreaseServiceViewCountRequest()
        {
            ServiceId = message.ServiceId,
        };
        
        await _coreApiCommunicator.CreateSeller(request, context.CancellationToken);
    }
}