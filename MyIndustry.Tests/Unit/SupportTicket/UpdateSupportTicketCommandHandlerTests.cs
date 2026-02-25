using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SupportTicket.UpdateSupportTicketCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainTicket = MyIndustry.Domain.Aggregate.SupportTicket;

namespace MyIndustry.Tests.Unit.SupportTicket;

public class UpdateSupportTicketCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainTicket>> _ticketRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateSupportTicketCommandHandler _handler;

    public UpdateSupportTicketCommandHandlerTests()
    {
        _ticketRepositoryMock = new Mock<IGenericRepository<DomainTicket>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateSupportTicketCommandHandler(_ticketRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Ticket_Not_Exists()
    {
        _ticketRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainTicket>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new UpdateSupportTicketCommand { Id = Guid.NewGuid(), Status = TicketStatus.InProgress, Priority = TicketPriority.Normal },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Destek talebi bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Update_Ticket_And_Return_Ok()
    {
        var ticketId = Guid.NewGuid();
        var ticket = new DomainTicket
        {
            Id = ticketId,
            Name = "Test",
            Email = "t@t.com",
            Subject = "S",
            Message = "M",
            Status = TicketStatus.Open,
            Priority = TicketPriority.Normal
        };
        _ticketRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainTicket> { ticket }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new UpdateSupportTicketCommand
            {
                Id = ticketId,
                Status = TicketStatus.Closed,
                Priority = TicketPriority.High,
                AdminNotes = "Not",
                AdminResponse = "Yanıt"
            },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.Priority.Should().Be(TicketPriority.High);
        ticket.AdminNotes.Should().Be("Not");
        ticket.AdminResponse.Should().Be("Yanıt");
    }
}
