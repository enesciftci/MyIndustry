using CoreApiCommunicator.Sms;
using FluentAssertions;
using MassTransit;
using Moq;
using MyIndustry.Queue;
using MyIndustry.Queue.Message;

namespace MyIndustry.Tests.Unit.Queue;

public class SendPhoneVerificationConsumerTests
{
    [Fact]
    public async Task Consume_Should_Send_Verification_Sms()
    {
        var smsSenderMock = new Mock<ISmsSender>();
        smsSenderMock.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var consumer = new SendPhoneVerificationConsumer(smsSenderMock.Object);
        var message = new SendPhoneVerificationMessage
        {
            PhoneNumber = "+905551234567",
            VerificationCode = "123456"
        };
        var contextMock = new Mock<ConsumeContext<SendPhoneVerificationMessage>>();
        contextMock.Setup(c => c.Message).Returns(message);

        await consumer.Consume(contextMock.Object);

        smsSenderMock.Verify(s => s.SendSmsAsync(
            "+905551234567",
            "MyIndustry dogrulama kodunuz: 123456. Bu kod 5 dakika gecerlidir."), Times.Once);
    }
}
