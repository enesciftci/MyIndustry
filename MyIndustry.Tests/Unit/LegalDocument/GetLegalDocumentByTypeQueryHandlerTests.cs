using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByTypeQuery;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.Tests.Unit.LegalDocument;

public class GetLegalDocumentByTypeQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainLegalDocument>> _legalDocumentRepositoryMock;
    private readonly GetLegalDocumentByTypeQueryHandler _handler;

    public GetLegalDocumentByTypeQueryHandlerTests()
    {
        _legalDocumentRepositoryMock = new Mock<IGenericRepository<DomainLegalDocument>>();
        _handler = new GetLegalDocumentByTypeQueryHandler(_legalDocumentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_LegalDocument_When_Active_And_Type_Matches()
    {
        var doc = new DomainLegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = LegalDocumentType.PrivacyPolicy,
            Title = "Gizlilik Politikası",
            Content = "İçerik",
            IsActive = true,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            DisplayOrder = 0
        };
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument> { doc }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetLegalDocumentByTypeQuery { DocumentType = (int)LegalDocumentType.PrivacyPolicy }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.LegalDocument.Should().NotBeNull();
        result.LegalDocument!.Title.Should().Be("Gizlilik Politikası");
        result.LegalDocument.DocumentType.Should().Be(LegalDocumentType.PrivacyPolicy);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_No_Active_Document_For_Type()
    {
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetLegalDocumentByTypeQuery { DocumentType = (int)LegalDocumentType.TermsOfService }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Sözleşme bulunamadı");
    }
}
