using CoreApiCommunicator;
using CoreApiCommunicator.Request;
using CoreApiCommunicator.Response;
using FluentAssertions;
using MassTransit;
using Moq;
using MyIndustry.Queue;
using MyIndustry.Queue.Message;

namespace MyIndustry.Tests.Unit.Queue;

public class IncreaseServiceViewCountConsumerTests
{
    [Fact]
    public async Task Consume_Should_Call_CoreApiCommunicator_With_ServiceId()
    {
        var communicatorMock = new Mock<ICoreApiCommunicator<IncreaseServiceViewCountRequest, ResponseBase>>();
        communicatorMock.Setup(c => c.CreateSeller(It.IsAny<IncreaseServiceViewCountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseBase { Success = true });

        var consumer = new IncreaseServiceViewCountConsumer(communicatorMock.Object);
        var serviceId = Guid.NewGuid();
        var message = new IncreaseServiceViewCountMessage { ServiceId = serviceId };
        var contextMock = new Mock<ConsumeContext<IncreaseServiceViewCountMessage>>();
        contextMock.Setup(c => c.Message).Returns(message);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        communicatorMock.Verify(c => c.CreateSeller(
            It.Is<IncreaseServiceViewCountRequest>(r => r.ServiceId == serviceId),
            CancellationToken.None), Times.Once);
    }
}
