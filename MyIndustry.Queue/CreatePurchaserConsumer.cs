using CoreApiCommunicator;
using CoreApiCommunicator.Request;
using CoreApiCommunicator.Response;
using MassTransit;
using MyIndustry.Queue.Message;

namespace MyIndustry.Queue;

public class CreatePurchaserConsumer : IConsumer<CreatePurchaserMessage>
{
    private readonly ICoreApiCommunicator<CreatePurchaserRequest,ResponseBase> _coreApiCommunicator;

    public CreatePurchaserConsumer(ICoreApiCommunicator<CreatePurchaserRequest, ResponseBase> coreApiCommunicator)
    {
        _coreApiCommunicator = coreApiCommunicator;
    }

    public async Task Consume(ConsumeContext<CreatePurchaserMessage> context)
    {
        var message = context.Message;
        var request = new CreatePurchaserRequest()
        {
            UserId = message.UserId,
            Email = message.Email,
            PhoneNumber = message.PhoneNumber
            // Address = message.Address,
            // City = message.City,
            // Description = message.Description,
            // District = message.District,
            // Email = message.Email,
            // Sector = message.Sector,
            // Title = message.Title,
            // AgreementUrl = message.AgreementUrl,
            // IdentityNumber = message.IdentityNumber,
            // PhoneNumber = message.PhoneNumber
        };
        
        await _coreApiCommunicator.CreatePurchaser(request, context.CancellationToken);
    }
}