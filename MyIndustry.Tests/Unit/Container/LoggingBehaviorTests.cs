using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.Container.Logging;
using MyIndustry.Container.MediatR;

namespace MyIndustry.Tests.Unit.Container;

public sealed record LoggingBehaviorTestQuery : IRequest<LoggingBehaviorTestQueryResult>;
public sealed record LoggingBehaviorTestQueryResult : ResponseBase;

public class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_Should_Skip_Logging_When_Disabled()
    {
        var logger = new Mock<ILogger<LoggingBehavior<LoggingBehaviorTestQuery, LoggingBehaviorTestQueryResult>>>();
        var options = Options.Create(new MediatRLoggingOptions { Enabled = false });
        var behavior = new LoggingBehavior<LoggingBehaviorTestQuery, LoggingBehaviorTestQueryResult>(logger.Object, options);
        var called = false;

        var result = await behavior.Handle(new LoggingBehaviorTestQuery(), () =>
        {
            called = true;
            return Task.FromResult(new LoggingBehaviorTestQueryResult());
        }, CancellationToken.None);

        called.Should().BeTrue();
        result.Should().NotBeNull();
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Log_And_Return_Response_When_Enabled()
    {
        var logger = new Mock<ILogger<LoggingBehavior<LoggingBehaviorTestQuery, LoggingBehaviorTestQueryResult>>>();
        var options = Options.Create(new MediatRLoggingOptions { Enabled = true, LogRequestPayload = false });
        var behavior = new LoggingBehavior<LoggingBehaviorTestQuery, LoggingBehaviorTestQueryResult>(logger.Object, options);
        var expected = new LoggingBehaviorTestQueryResult { Success = true, MessageCode = "0000" };

        var result = await behavior.Handle(new LoggingBehaviorTestQuery(), () => Task.FromResult(expected), CancellationToken.None);

        result.Should().BeSameAs(expected);
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_Should_Log_Warning_And_Rethrow_On_Exception()
    {
        var logger = new Mock<ILogger<LoggingBehavior<LoggingBehaviorTestQuery, LoggingBehaviorTestQueryResult>>>();
        var options = Options.Create(new MediatRLoggingOptions { Enabled = true, LogRequestPayload = false });
        var behavior = new LoggingBehavior<LoggingBehaviorTestQuery, LoggingBehaviorTestQueryResult>(logger.Object, options);

        var act = () => behavior.Handle(new LoggingBehaviorTestQuery(), () => throw new InvalidOperationException("fail"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("fail");
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
