using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.LegalDocument.CreateLegalDocumentCommand;
using MyIndustry.ApplicationService.Handler.LegalDocument.DeleteLegalDocumentCommand;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetActiveLegalDocumentsByTypesQuery;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByTypeQuery;
using MyIndustry.ApplicationService.Handler.LegalDocument.UpdateLegalDocumentCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.LegalDocument;

public class LegalDocumentIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public LegalDocumentIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _repository = new GenericRepository<Domain.Aggregate.LegalDocument>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task CreateLegalDocument_Should_Save_To_Database()
    {
        var handler = new CreateLegalDocumentCommandHandler(_repository, _unitOfWork);
        var result = await handler.Handle(new CreateLegalDocumentCommand
        {
            LegalDocumentDto = new LegalDocumentDto
            {
                DocumentType = LegalDocumentType.KVKK,
                Title = "KVKK Aydınlatma Metni",
                Content = "Kişisel verilerin korunması hakkında bilgilendirme.",
                Version = "1.0",
                IsActive = true,
                EffectiveDate = DateTime.UtcNow,
                DisplayOrder = 1
            }
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var saved = await _context.LegalDocuments
            .FirstOrDefaultAsync(d => d.Title == "KVKK Aydınlatma Metni");
        saved.Should().NotBeNull();
        saved!.DocumentType.Should().Be(LegalDocumentType.KVKK);
    }

    [Fact]
    public async Task GetActiveLegalDocuments_Should_Return_Only_Active_Documents()
    {
        await TestDataBuilder.SeedLegalDocumentAsync(_context, LegalDocumentType.KVKK, "Active KVKK");
        _context.LegalDocuments.Add(new Domain.Aggregate.LegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = LegalDocumentType.PrivacyPolicy,
            Title = "Inactive Policy",
            Content = "Inactive",
            IsActive = false,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new GetActiveLegalDocumentsByTypesQueryHandler(_repository);
        var result = await handler.Handle(new GetActiveLegalDocumentsByTypesQuery
        {
            DocumentTypes = new List<int> { (int)LegalDocumentType.KVKK, (int)LegalDocumentType.PrivacyPolicy }
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.LegalDocuments.Should().Contain(d => d.Title == "Active KVKK");
        result.LegalDocuments.Should().NotContain(d => d.Title == "Inactive Policy");
    }

    [Fact]
    public async Task GetLegalDocumentByType_Should_Return_Latest_Active()
    {
        await TestDataBuilder.SeedLegalDocumentAsync(_context, LegalDocumentType.TermsOfService, "Terms v1");

        var handler = new GetLegalDocumentByTypeQueryHandler(_repository);
        var result = await handler.Handle(
            new GetLegalDocumentByTypeQuery { DocumentType = (int)LegalDocumentType.TermsOfService },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.LegalDocument.Should().NotBeNull();
        result.LegalDocument!.Title.Should().Be("Terms v1");
    }

    [Fact]
    public async Task UpdateLegalDocument_Should_Persist_Changes()
    {
        var existing = await TestDataBuilder.SeedLegalDocumentAsync(_context);
        var handler = new UpdateLegalDocumentCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(new UpdateLegalDocumentCommand
        {
            LegalDocumentDto = new LegalDocumentDto
            {
                Id = existing.Id,
                DocumentType = existing.DocumentType,
                Title = "Updated Title",
                Content = "Updated content",
                Version = "2.0",
                IsActive = true,
                DisplayOrder = 2
            }
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var updated = await _context.LegalDocuments.FindAsync(existing.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Version.Should().Be("2.0");
        updated.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteLegalDocument_Should_Remove_From_Database()
    {
        var document = await TestDataBuilder.SeedLegalDocumentAsync(_context);
        var handler = new DeleteLegalDocumentCommandHandler(_repository, _unitOfWork);

        var result = await handler.Handle(new DeleteLegalDocumentCommand { Id = document.Id }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var deleted = await _context.LegalDocuments.FindAsync(document.Id);
        deleted.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
