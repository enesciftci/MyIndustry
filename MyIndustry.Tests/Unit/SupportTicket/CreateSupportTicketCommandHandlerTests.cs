using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainTicket = MyIndustry.Domain.Aggregate.SupportTicket;

namespace MyIndustry.Tests.Unit.SupportTicket;

public class CreateSupportTicketCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainTicket>> _ticketRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateSupportTicketCommandHandler _handler;

    public CreateSupportTicketCommandHandlerTests()
    {
        _ticketRepositoryMock = new Mock<IGenericRepository<DomainTicket>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateSupportTicketCommandHandler(_ticketRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Ticket_And_Return_Ok()
    {
        _ticketRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainTicket>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var userId = Guid.NewGuid();
        var command = new CreateSupportTicketCommand
        {
            UserId = userId,
            UserType = 1,
            Name = "Test User",
            Email = "test@test.com",
            Subject = "Konu",
            Message = "Mesaj içeriği",
            Category = TicketCategory.General
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.TicketId.Should().NotBeEmpty();
        _ticketRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainTicket>(t =>
            t.UserId == userId &&
            t.Name == "Test User" &&
            t.Email == "test@test.com" &&
            t.Subject == "Konu" &&
            t.Message == "Mesaj içeriği" &&
            t.Category == TicketCategory.General &&
            t.Status == TicketStatus.Open &&
            t.Priority == TicketPriority.Normal
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_Should_Return_BadRequest_When_Name_Invalid(string name)
    {
        var command = new CreateSupportTicketCommand
        {
            Name = name!,
            Email = "test@test.com",
            Subject = "Konu",
            Message = "Mesaj",
            Category = TicketCategory.General
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Ad");
        _ticketRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainTicket>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Email_Empty()
    {
        var command = new CreateSupportTicketCommand
        {
            Name = "Test",
            Email = "  ",
            Subject = "Konu",
            Message = "Mesaj",
            Category = TicketCategory.General
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("E-posta");
        _ticketRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainTicket>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Subject_Empty()
    {
        var command = new CreateSupportTicketCommand
        {
            Name = "Test",
            Email = "a@a.com",
            Subject = "",
            Message = "Mesaj",
            Category = TicketCategory.General
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Konu");
        _ticketRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainTicket>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Message_Empty()
    {
        var command = new CreateSupportTicketCommand
        {
            Name = "Test",
            Email = "a@a.com",
            Subject = "Konu",
            Message = null!,
            Category = TicketCategory.General
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Mesaj");
        _ticketRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainTicket>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
