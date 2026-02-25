using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Message.GetUnreadCountQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;

namespace MyIndustry.Tests.Unit.Message;

public class GetUnreadCountQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainMessage>> _messageRepositoryMock;
    private readonly GetUnreadCountQueryHandler _handler;

    public GetUnreadCountQueryHandlerTests()
    {
        _messageRepositoryMock = new Mock<IGenericRepository<DomainMessage>>();
        _handler = new GetUnreadCountQueryHandler(_messageRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Zero_When_No_Unread()
    {
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetUnreadCountQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Count_When_Unread_Exists()
    {
        var userId = Guid.NewGuid();
        var messages = new List<DomainMessage>
        {
            new() { Id = Guid.NewGuid(), ReceiverId = userId, IsRead = false, Content = "", SenderName = "", SenderEmail = "" },
            new() { Id = Guid.NewGuid(), ReceiverId = userId, IsRead = false, Content = "", SenderName = "", SenderEmail = "" }
        };
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(messages.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetUnreadCountQuery { UserId = userId }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.UnreadCount.Should().Be(2);
    }
}
