using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByIdQuery;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.Tests.Unit.LegalDocument;

public class GetLegalDocumentByIdQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainLegalDocument>> _legalDocumentRepositoryMock;
    private readonly GetLegalDocumentByIdQueryHandler _handler;

    public GetLegalDocumentByIdQueryHandlerTests()
    {
        _legalDocumentRepositoryMock = new Mock<IGenericRepository<DomainLegalDocument>>();
        _handler = new GetLegalDocumentByIdQueryHandler(_legalDocumentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_LegalDocument_When_Found()
    {
        var id = Guid.NewGuid();
        var doc = new DomainLegalDocument
        {
            Id = id,
            DocumentType = LegalDocumentType.TermsOfService,
            Title = "Kullanım Şartları",
            Content = "İçerik",
            IsActive = true,
            DisplayOrder = 0
        };
        var list = new List<DomainLegalDocument> { doc }.AsQueryable().BuildMock();
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(list);

        var query = new GetLegalDocumentByIdQuery { Id = id };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.LegalDocument.Should().NotBeNull();
        result.LegalDocument!.Id.Should().Be(id);
        result.LegalDocument.Title.Should().Be("Kullanım Şartları");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Document_Does_Not_Exist()
    {
        var emptyList = new List<DomainLegalDocument>().AsQueryable().BuildMock();
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(emptyList);

        var query = new GetLegalDocumentByIdQuery { Id = Guid.NewGuid() };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }
}
