using CoreApiCommunicator.Email;
using FluentAssertions;
using MassTransit;
using Moq;
using MyIndustry.Queue;
using MyIndustry.Queue.Message;

namespace MyIndustry.Tests.Unit.Queue;

public class SendForgotPasswordEmailConsumerTests
{
    [Fact]
    public async Task Consume_Should_Send_Forgot_Password_Email()
    {
        var emailSenderMock = new Mock<IEmailSender>();
        var consumer = new SendForgotPasswordEmailConsumer(emailSenderMock.Object);
        var message = new SendForgotPasswordEmailMessage
        {
            Email = "user@example.com",
            Subject = "Reset password",
            Body = "<html>Reset</html>"
        };
        var contextMock = new Mock<ConsumeContext<SendForgotPasswordEmailMessage>>();
        contextMock.Setup(c => c.Message).Returns(message);

        await consumer.Consume(contextMock.Object);

        emailSenderMock.Verify(e => e.SendEmailAsync(
            "user@example.com",
            "Reset password",
            "<html>Reset</html>"), Times.Once);
    }
}
