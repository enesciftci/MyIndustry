using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Message.MarkMessagesAsReadCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;

namespace MyIndustry.Tests.Unit.Message;

public class MarkMessagesAsReadCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainMessage>> _messageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkMessagesAsReadCommandHandler _handler;

    public MarkMessagesAsReadCommandHandlerTests()
    {
        _messageRepositoryMock = new Mock<IGenericRepository<DomainMessage>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new MarkMessagesAsReadCommandHandler(_messageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Zero_MarkedCount_When_No_Unread()
    {
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new MarkMessagesAsReadCommand { UserId = Guid.NewGuid(), ServiceId = Guid.NewGuid(), OtherUserId = Guid.NewGuid() },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.MarkedCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Mark_Unread_And_Return_Count()
    {
        var userId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var unreadMessage = new DomainMessage
        {
            Id = Guid.NewGuid(),
            ServiceId = serviceId,
            SenderId = otherUserId,
            ReceiverId = userId,
            IsRead = false,
            Content = "x",
            SenderName = "A",
            SenderEmail = "a@x.com"
        };
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage> { unreadMessage }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new MarkMessagesAsReadCommand { UserId = userId, ServiceId = serviceId, OtherUserId = otherUserId },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.MarkedCount.Should().Be(1);
        unreadMessage.IsRead.Should().BeTrue();
        _messageRepositoryMock.Verify(r => r.Update(unreadMessage), Times.Once);
    }
}
