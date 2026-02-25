using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Message.ReplyMessageCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Message;

public class ReplyMessageCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainMessage>> _messageRepositoryMock;
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ReplyMessageCommandHandler _handler;

    public ReplyMessageCommandHandlerTests()
    {
        _messageRepositoryMock = new Mock<IGenericRepository<DomainMessage>>();
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ReplyMessageCommandHandler(
            _messageRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Service_Not_Exists()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new ReplyMessageCommand { UserId = Guid.NewGuid(), UserName = "A", UserEmail = "a@x.com", ServiceId = Guid.NewGuid(), ReceiverId = Guid.NewGuid(), Content = "Test" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("İlan bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_User_Replies_To_Self()
    {
        var userId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { new DomainService { Id = serviceId } }.AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new ReplyMessageCommand { UserId = userId, UserName = "A", UserEmail = "a@x.com", ServiceId = serviceId, ReceiverId = userId, Content = "Test" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Kendinize mesaj gönderemezsiniz");
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_No_Existing_Conversation()
    {
        var serviceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { new DomainService { Id = serviceId } }.AsQueryable().BuildMock());
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new ReplyMessageCommand { UserId = userId, UserName = "A", UserEmail = "a@x.com", ServiceId = serviceId, ReceiverId = receiverId, Content = "Test" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Bu konuşmada yanıt veremezsiniz");
    }

    [Fact]
    public async Task Handle_Should_Add_Reply_And_Return_Ok_When_Conversation_Exists()
    {
        var serviceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var existingMessage = new DomainMessage { ServiceId = serviceId, SenderId = userId, ReceiverId = receiverId, Content = "", SenderName = "", SenderEmail = "" };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { new DomainService { Id = serviceId } }.AsQueryable().BuildMock());
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage> { existingMessage }.AsQueryable().BuildMock());
        _messageRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new ReplyMessageCommand { UserId = userId, UserName = "A", UserEmail = "a@x.com", ServiceId = serviceId, ReceiverId = receiverId, Content = "Yanıt" },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.MessageId.Should().NotBeEmpty();
        _messageRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainMessage>(m =>
            m.ServiceId == serviceId && m.SenderId == userId && m.ReceiverId == receiverId && m.Content == "Yanıt"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
