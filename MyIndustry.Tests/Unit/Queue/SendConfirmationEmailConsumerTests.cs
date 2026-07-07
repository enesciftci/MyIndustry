using CoreApiCommunicator.Email;
using FluentAssertions;
using MassTransit;
using Moq;
using MyIndustry.Queue;
using MyIndustry.Queue.Message;

namespace MyIndustry.Tests.Unit.Queue;

public class SendConfirmationEmailConsumerTests
{
    [Fact]
    public async Task Consume_Should_Send_Confirmation_Email()
    {
        var emailSenderMock = new Mock<IEmailSender>();
        var consumer = new SendConfirmationEmailConsumer(emailSenderMock.Object);
        var message = new SendConfirmationEmailMessage
        {
            Email = "user@example.com",
            Subject = "Confirm your account",
            Body = "<html>Confirm</html>"
        };
        var contextMock = new Mock<ConsumeContext<SendConfirmationEmailMessage>>();
        contextMock.Setup(c => c.Message).Returns(message);

        await consumer.Consume(contextMock.Object);

        emailSenderMock.Verify(e => e.SendEmailAsync(
            "user@example.com",
            "Confirm your account",
            "<html>Confirm</html>"), Times.Once);
    }
}
