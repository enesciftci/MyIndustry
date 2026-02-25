using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetActiveLegalDocumentsByTypesQuery;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.Tests.Unit.LegalDocument;

public class GetActiveLegalDocumentsByTypesQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainLegalDocument>> _legalDocumentRepositoryMock;
    private readonly GetActiveLegalDocumentsByTypesQueryHandler _handler;

    public GetActiveLegalDocumentsByTypesQueryHandlerTests()
    {
        _legalDocumentRepositoryMock = new Mock<IGenericRepository<DomainLegalDocument>>();
        _handler = new GetActiveLegalDocumentsByTypesQueryHandler(_legalDocumentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Documents()
    {
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetActiveLegalDocumentsByTypesQuery { DocumentTypes = new List<int> { (int)LegalDocumentType.KVKK } }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.LegalDocuments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Active_Documents_For_Requested_Types()
    {
        var doc = new DomainLegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = LegalDocumentType.TermsOfService,
            Title = "Kullanım Şartları",
            Content = "İçerik",
            IsActive = true,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = null,
            DisplayOrder = 0
        };
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument> { doc }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetActiveLegalDocumentsByTypesQuery { DocumentTypes = new List<int> { (int)LegalDocumentType.TermsOfService } }, CancellationToken.None);

        result.LegalDocuments.Should().HaveCount(1);
        result.LegalDocuments![0].Title.Should().Be("Kullanım Şartları");
    }

    [Fact]
    public async Task Handle_When_DocumentTypes_Null_Or_Empty_Should_Use_Default_Types()
    {
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetActiveLegalDocumentsByTypesQuery { DocumentTypes = new List<int>() }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.LegalDocuments.Should().NotBeNull();
    }
}
