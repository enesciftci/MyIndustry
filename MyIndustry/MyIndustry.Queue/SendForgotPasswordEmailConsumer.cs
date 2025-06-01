using CoreApiCommunicator.Email;
using MassTransit;
using MyIndustry.Queue.Message;

namespace MyIndustry.Queue;

public class SendForgotPasswordEmailConsumer : IConsumer<SendForgotPasswordEmailMessage>
{
    private readonly IEmailSender _emailSender;

    public SendForgotPasswordEmailConsumer(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Consume(ConsumeContext<SendForgotPasswordEmailMessage> context)
    {
        var message = context.Message;
        Console.WriteLine(message.Body);
        await _emailSender.SendEmailAsync(message.Email, message.Subject, message.Body);
    }
}