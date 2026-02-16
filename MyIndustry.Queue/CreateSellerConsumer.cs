using CoreApiCommunicator;
using CoreApiCommunicator.Request;
using CoreApiCommunicator.Response;
using MassTransit;
using MyIndustry.Queue.Message;

namespace MyIndustry.Queue;

public class CreateSellerConsumer : IConsumer<CreateSellerMessage>
{
    private readonly ICoreApiCommunicator<CreateSellerRequest,ResponseBase> _coreApiCommunicator;

    public CreateSellerConsumer(ICoreApiCommunicator<CreateSellerRequest,ResponseBase> coreApiCommunicator)
    {
        _coreApiCommunicator = coreApiCommunicator;
    }

    public async Task Consume(ConsumeContext<CreateSellerMessage> context)
    {
        var message = context.Message;
        var request = new CreateSellerRequest()
        {
            UserId = message.UserId,
            Address = message.Address,
            City = message.City,
            Description = message.Description,
            District = message.District,
            Email = message.Email,
            Sector = message.Sector,
            Title = message.Title,
            AgreementUrl = message.AgreementUrl,
            IdentityNumber = message.IdentityNumber,
            PhoneNumber = message.PhoneNumber
        };
        
        await _coreApiCommunicator.CreateSeller(request, context.CancellationToken);
    }
}