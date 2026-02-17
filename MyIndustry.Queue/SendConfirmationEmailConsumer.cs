using CoreApiCommunicator.Email;
using MassTransit;
using MyIndustry.Queue.Message;

namespace MyIndustry.Queue;

public class SendConfirmationEmailConsumer : IConsumer<SendConfirmationEmailMessage>
{
    private readonly IEmailSender _emailSender;

    public SendConfirmationEmailConsumer(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Consume(ConsumeContext<SendConfirmationEmailMessage> context)
    {
        var message = context.Message;
        Console.WriteLine($"Sending confirmation email to: {message.Email}");
        await _emailSender.SendEmailAsync(message.Email, message.Subject, message.Body);
        Console.WriteLine($"Confirmation email sent to: {message.Email}");
    }
}
