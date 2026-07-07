using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.UserLegalDocumentAcceptance.SaveUserLegalDocumentAcceptancesCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Integration.UserLegalDocumentAcceptance;

public class UserLegalDocumentAcceptanceIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<Domain.Aggregate.UserLegalDocumentAcceptance> _acceptanceRepository;

    public UserLegalDocumentAcceptanceIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _acceptanceRepository = new GenericRepository<Domain.Aggregate.UserLegalDocumentAcceptance>(_context);
    }

    [Fact]
    public async Task SaveAcceptances_Should_Track_Documents_For_User()
    {
        var doc1 = await TestDataBuilder.SeedLegalDocumentAsync(_context);
        var doc2 = await TestDataBuilder.SeedLegalDocumentAsync(_context, title: "Second Document");
        var userId = Guid.NewGuid();

        var handler = new SaveUserLegalDocumentAcceptancesCommandHandler(_acceptanceRepository);
        var result = await handler.Handle(new SaveUserLegalDocumentAcceptancesCommand
        {
            UserId = userId,
            LegalDocumentIds = new List<Guid> { doc1.Id, doc2.Id }
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _context.SaveChangesAsync();
        var acceptances = await _context.UserLegalDocumentAcceptances
            .Where(a => a.UserId == userId)
            .ToListAsync();
        acceptances.Should().HaveCount(2);
        acceptances.Should().OnlyContain(a => a.AcceptedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task SaveAcceptances_Empty_List_Should_Return_Ok()
    {
        var handler = new SaveUserLegalDocumentAcceptancesCommandHandler(_acceptanceRepository);
        var result = await handler.Handle(new SaveUserLegalDocumentAcceptancesCommand
        {
            UserId = Guid.NewGuid(),
            LegalDocumentIds = new List<Guid>()
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var count = await _context.UserLegalDocumentAcceptances.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task SaveAcceptances_Should_Deduplicate_Document_Ids()
    {
        var doc = await TestDataBuilder.SeedLegalDocumentAsync(_context);
        var userId = Guid.NewGuid();

        var handler = new SaveUserLegalDocumentAcceptancesCommandHandler(_acceptanceRepository);
        await handler.Handle(new SaveUserLegalDocumentAcceptancesCommand
        {
            UserId = userId,
            LegalDocumentIds = new List<Guid> { doc.Id, doc.Id, doc.Id }
        }, CancellationToken.None);

        await _context.SaveChangesAsync();
        var acceptances = await _context.UserLegalDocumentAcceptances
            .Where(a => a.UserId == userId)
            .ToListAsync();
        acceptances.Should().HaveCount(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
