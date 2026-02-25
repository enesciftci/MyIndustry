using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetAllLegalDocumentsQuery;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.Tests.Unit.LegalDocument;

public class GetAllLegalDocumentsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainLegalDocument>> _legalDocumentRepositoryMock;
    private readonly GetAllLegalDocumentsQueryHandler _handler;

    public GetAllLegalDocumentsQueryHandlerTests()
    {
        _legalDocumentRepositoryMock = new Mock<IGenericRepository<DomainLegalDocument>>();
        _handler = new GetAllLegalDocumentsQueryHandler(_legalDocumentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Documents()
    {
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAllLegalDocumentsQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.LegalDocuments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Documents_Ordered_By_DisplayOrder_And_CreatedDate()
    {
        var doc1 = new DomainLegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = LegalDocumentType.KVKK,
            Title = "KVKK",
            Content = "İçerik",
            DisplayOrder = 1,
            CreatedDate = DateTime.UtcNow
        };
        var doc2 = new DomainLegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = LegalDocumentType.TermsOfService,
            Title = "Kullanım Şartları",
            Content = "İçerik",
            DisplayOrder = 0,
            CreatedDate = DateTime.UtcNow
        };
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument> { doc1, doc2 }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAllLegalDocumentsQuery(), CancellationToken.None);

        result.LegalDocuments.Should().HaveCount(2);
        result.LegalDocuments!.Select(d => d.Title).Should().Contain("KVKK").And.Contain("Kullanım Şartları");
    }
}
