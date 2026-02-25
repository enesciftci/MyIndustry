using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Message.GetConversationMessagesQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Message;

public class GetConversationMessagesQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainMessage>> _messageRepositoryMock;
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly GetConversationMessagesQueryHandler _handler;

    public GetConversationMessagesQueryHandlerTests()
    {
        _messageRepositoryMock = new Mock<IGenericRepository<DomainMessage>>();
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _handler = new GetConversationMessagesQueryHandler(_messageRepositoryMock.Object, _serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Service_Not_Exists()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new GetConversationMessagesQuery { UserId = Guid.NewGuid(), ServiceId = Guid.NewGuid(), OtherUserId = Guid.NewGuid() },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("İlan bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Return_Messages_When_Service_Exists()
    {
        var serviceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var service = new DomainService { Id = serviceId, Title = "İlan", ImageUrls = "http://img.jpg" };
        var messages = new List<DomainMessage>
        {
            new() { Id = Guid.NewGuid(), ServiceId = serviceId, SenderId = userId, ReceiverId = otherUserId, Content = "Merhaba", SenderName = "A", SenderEmail = "a@x.com", IsRead = true, CreatedDate = DateTime.UtcNow.AddHours(-1) },
            new() { Id = Guid.NewGuid(), ServiceId = serviceId, SenderId = otherUserId, ReceiverId = userId, Content = "Selam", SenderName = "B", SenderEmail = "b@x.com", IsRead = false, CreatedDate = DateTime.UtcNow }
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(messages.AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new GetConversationMessagesQuery { UserId = userId, ServiceId = serviceId, OtherUserId = otherUserId },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Messages.Should().HaveCount(2);
        result.ServiceTitle.Should().Be("İlan");
    }
}
