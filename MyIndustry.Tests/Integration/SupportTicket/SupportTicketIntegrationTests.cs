using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;
using MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;
using MyIndustry.ApplicationService.Handler.SupportTicket.UpdateSupportTicketCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.SupportTicket;

public class SupportTicketIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<Domain.Aggregate.SupportTicket> _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SupportTicketIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _ticketRepository = new GenericRepository<Domain.Aggregate.SupportTicket>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task CreateSupportTicket_Should_Save_To_Database()
    {
        var handler = new CreateSupportTicketCommandHandler(_ticketRepository, _unitOfWork);
        var result = await handler.Handle(new CreateSupportTicketCommand
        {
            UserId = Guid.NewGuid(),
            UserType = 0,
            Name = "Test User",
            Email = "support@example.com",
            Phone = "+905551234567",
            Subject = "Account issue",
            Message = "I cannot access my account.",
            Category = TicketCategory.General
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.TicketId.Should().NotBeEmpty();
        var saved = await _context.SupportTickets.FindAsync(result.TicketId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(TicketStatus.Open);
        saved.Priority.Should().Be(TicketPriority.Normal);
    }

    [Fact]
    public async Task CreateSupportTicket_Missing_Fields_Should_Return_BadRequest()
    {
        var handler = new CreateSupportTicketCommandHandler(_ticketRepository, _unitOfWork);
        var result = await handler.Handle(new CreateSupportTicketCommand
        {
            Name = "",
            Email = "support@example.com",
            Subject = "Test",
            Message = "Test"
        }, CancellationToken.None);

        result.Success.Should().BeFalse();
        var count = await _context.SupportTickets.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetSupportTickets_Should_Filter_By_Status()
    {
        _context.SupportTickets.AddRange(
            new Domain.Aggregate.SupportTicket
            {
                Id = Guid.NewGuid(),
                Name = "User 1",
                Email = "u1@example.com",
                Subject = "Open ticket",
                Message = "Help",
                Category = TicketCategory.General,
                Status = TicketStatus.Open,
                Priority = TicketPriority.Normal,
                CreatedDate = DateTime.UtcNow
            },
            new Domain.Aggregate.SupportTicket
            {
                Id = Guid.NewGuid(),
                Name = "User 2",
                Email = "u2@example.com",
                Subject = "Closed ticket",
                Message = "Resolved",
                Category = TicketCategory.General,
                Status = TicketStatus.Closed,
                Priority = TicketPriority.Low,
                CreatedDate = DateTime.UtcNow
            });
        await _context.SaveChangesAsync();

        var handler = new GetSupportTicketsQueryHandler(_ticketRepository);
        var result = await handler.Handle(new GetSupportTicketsQuery
        {
            Status = TicketStatus.Open,
            Index = 1,
            Size = 10
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tickets.Should().HaveCount(1);
        result.Tickets[0].Subject.Should().Be("Open ticket");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateSupportTicket_Should_Return_Success()
    {
        var ticketId = Guid.NewGuid();
        _context.SupportTickets.Add(new Domain.Aggregate.SupportTicket
        {
            Id = ticketId,
            Name = "User",
            Email = "user@example.com",
            Subject = "Issue",
            Message = "Need help",
            Category = TicketCategory.Technical,
            Status = TicketStatus.Open,
            Priority = TicketPriority.Normal,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new UpdateSupportTicketCommandHandler(_ticketRepository, _unitOfWork);
        var result = await handler.Handle(new UpdateSupportTicketCommand
        {
            Id = ticketId,
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.High,
            AdminNotes = "Investigating",
            AdminResponse = "We are looking into this."
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
