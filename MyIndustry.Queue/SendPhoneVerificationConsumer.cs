using CoreApiCommunicator.Sms;
using MassTransit;
using MyIndustry.Queue.Message;

namespace MyIndustry.Queue;

public class SendPhoneVerificationConsumer : IConsumer<SendPhoneVerificationMessage>
{
    private readonly ISmsSender _smsSender;

    public SendPhoneVerificationConsumer(ISmsSender smsSender)
    {
        _smsSender = smsSender;
    }

    public async Task Consume(ConsumeContext<SendPhoneVerificationMessage> context)
    {
        var message = context.Message;
        Console.WriteLine($"Sending phone verification to: {message.PhoneNumber}");

        var smsMessage = $"MyIndustry dogrulama kodunuz: {message.VerificationCode}. Bu kod 5 dakika gecerlidir.";
        
        await _smsSender.SendSmsAsync(message.PhoneNumber, smsMessage);
        Console.WriteLine($"Phone verification sent to: {message.PhoneNumber}");
    }
}
