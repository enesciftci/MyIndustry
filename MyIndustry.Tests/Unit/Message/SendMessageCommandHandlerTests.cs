using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Message;

public class SendMessageCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainMessage>> _messageRepositoryMock;
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SendMessageCommandHandler _handler;

    public SendMessageCommandHandlerTests()
    {
        _messageRepositoryMock = new Mock<IGenericRepository<DomainMessage>>();
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new SendMessageCommandHandler(
            _messageRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Service_Not_Exists()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new SendMessageCommand { ServiceId = Guid.NewGuid(), SenderId = Guid.NewGuid(), SenderName = "A", SenderEmail = "a@x.com", Content = "Test" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("İlan bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Sender_Is_Seller()
    {
        var sellerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var service = new DomainService { Id = serviceId, SellerId = sellerId };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new SendMessageCommand { ServiceId = serviceId, SenderId = sellerId, SenderName = "S", SenderEmail = "s@x.com", Content = "Test" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Kendi ilanınıza mesaj gönderemezsiniz");
    }

    [Fact]
    public async Task Handle_Should_Add_Message_And_Return_Ok_When_Valid()
    {
        var serviceId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var service = new DomainService { Id = serviceId, SellerId = sellerId };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());
        _messageRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new SendMessageCommand { ServiceId = serviceId, SenderId = senderId, SenderName = "A", SenderEmail = "a@x.com", Content = "Merhaba" },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.MessageId.Should().NotBeEmpty();
        _messageRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainMessage>(m =>
            m.ServiceId == serviceId && m.SenderId == senderId && m.ReceiverId == sellerId && m.Content == "Merhaba" && !m.IsRead
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
