using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainTicket = MyIndustry.Domain.Aggregate.SupportTicket;

namespace MyIndustry.Tests.Unit.SupportTicket;

public class GetSupportTicketsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainTicket>> _ticketRepositoryMock;
    private readonly GetSupportTicketsQueryHandler _handler;

    public GetSupportTicketsQueryHandlerTests()
    {
        _ticketRepositoryMock = new Mock<IGenericRepository<DomainTicket>>();
        _handler = new GetSupportTicketsQueryHandler(_ticketRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Tickets()
    {
        _ticketRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainTicket>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSupportTicketsQuery { Index = 1, Size = 20 }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tickets.Should().NotBeNull().And.BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Tickets_With_Pagination()
    {
        var ticket = new DomainTicket
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "t@t.com",
            Subject = "S",
            Message = "M",
            Category = TicketCategory.General,
            Status = TicketStatus.Open,
            Priority = TicketPriority.Normal,
            CreatedDate = DateTime.UtcNow
        };
        _ticketRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainTicket> { ticket }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSupportTicketsQuery { Index = 1, Size = 10 }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tickets.Should().HaveCount(1);
        result.Tickets![0].Subject.Should().Be("S");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Status_When_Provided()
    {
        var openTicket = new DomainTicket
        {
            Id = Guid.NewGuid(),
            Name = "A",
            Email = "a@a.com",
            Subject = "Open",
            Message = "M",
            Category = TicketCategory.General,
            Status = TicketStatus.Open,
            Priority = TicketPriority.Normal,
            CreatedDate = DateTime.UtcNow
        };
        var closedTicket = new DomainTicket
        {
            Id = Guid.NewGuid(),
            Name = "B",
            Email = "b@b.com",
            Subject = "Closed",
            Message = "M",
            Category = TicketCategory.General,
            Status = TicketStatus.Closed,
            Priority = TicketPriority.Normal,
            CreatedDate = DateTime.UtcNow
        };
        _ticketRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainTicket> { openTicket, closedTicket }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSupportTicketsQuery { Index = 1, Size = 20, Status = TicketStatus.Open }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tickets.Should().HaveCount(1);
        result.Tickets![0].Status.Should().Be(TicketStatus.Open);
        result.TotalCount.Should().Be(1);
    }
}
