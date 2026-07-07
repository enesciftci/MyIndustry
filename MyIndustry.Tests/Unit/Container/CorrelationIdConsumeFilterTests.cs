using FluentAssertions;
using MassTransit;
using Moq;
using MyIndustry.Container.Logging;
using MyIndustry.Container.MassTransit;

namespace MyIndustry.Tests.Unit.Container;

public class CorrelationIdConsumeFilterTests
{
    public sealed class TestMessage
    {
        public string Value { get; set; } = "test";
    }

    [Fact]
    public async Task Send_Should_Use_Header_Correlation_Id()
    {
        const string correlationId = "consume-correlation-id";
        var contextMock = new Mock<ConsumeContext<TestMessage>>();
        var headersMock = new Mock<Headers>();
        headersMock.Setup(h => h.Get<string>(CorrelationIdConstants.HeaderName, null)).Returns(correlationId);
        contextMock.Setup(c => c.Headers).Returns(headersMock.Object);

        string? capturedId = null;
        var pipeMock = new Mock<IPipe<ConsumeContext<TestMessage>>>();
        pipeMock.Setup(p => p.Send(It.IsAny<ConsumeContext<TestMessage>>()))
            .Callback(() => capturedId = CorrelationIdContext.Current)
            .Returns(Task.CompletedTask);

        var filter = new CorrelationIdConsumeFilter<TestMessage>();
        await filter.Send(contextMock.Object, pipeMock.Object);

        capturedId.Should().Be(correlationId);
        pipeMock.Verify(p => p.Send(contextMock.Object), Times.Once);
    }

    [Fact]
    public async Task Send_Should_Use_MassTransit_CorrelationId_When_Header_Missing()
    {
        var massTransitId = Guid.NewGuid();
        var contextMock = new Mock<ConsumeContext<TestMessage>>();
        var headersMock = new Mock<Headers>();
        headersMock.Setup(h => h.Get<string>(CorrelationIdConstants.HeaderName, null)).Returns((string?)null);
        contextMock.Setup(c => c.Headers).Returns(headersMock.Object);
        contextMock.Setup(c => c.CorrelationId).Returns(massTransitId);

        string? capturedId = null;
        var pipeMock = new Mock<IPipe<ConsumeContext<TestMessage>>>();
        pipeMock.Setup(p => p.Send(It.IsAny<ConsumeContext<TestMessage>>()))
            .Callback(() => capturedId = CorrelationIdContext.Current)
            .Returns(Task.CompletedTask);

        var filter = new CorrelationIdConsumeFilter<TestMessage>();
        await filter.Send(contextMock.Object, pipeMock.Object);

        capturedId.Should().Be(massTransitId.ToString());
    }
}
