using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.LegalDocument.UpdateLegalDocumentCommand;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.Tests.Unit.LegalDocument;

public class UpdateLegalDocumentCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainLegalDocument>> _legalDocumentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateLegalDocumentCommandHandler _handler;

    public UpdateLegalDocumentCommandHandlerTests()
    {
        _legalDocumentRepositoryMock = new Mock<IGenericRepository<DomainLegalDocument>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateLegalDocumentCommandHandler(_legalDocumentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Document_When_Found()
    {
        var id = Guid.NewGuid();
        var doc = new DomainLegalDocument
        {
            Id = id,
            DocumentType = LegalDocumentType.TermsOfService,
            Title = "Eski Başlık",
            Content = "Eski içerik",
            IsActive = true
        };
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument> { doc }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var dto = new LegalDocumentDto
        {
            Id = id,
            DocumentType = LegalDocumentType.PrivacyPolicy,
            Title = "Yeni Başlık",
            Content = "Yeni içerik",
            IsActive = true,
            DisplayOrder = 1
        };
        var command = new UpdateLegalDocumentCommand { LegalDocumentDto = dto };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        doc.Title.Should().Be("Yeni Başlık");
        doc.Content.Should().Be("Yeni içerik");
        doc.DocumentType.Should().Be(LegalDocumentType.PrivacyPolicy);
        _legalDocumentRepositoryMock.Verify(r => r.Update(doc), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Document_Does_Not_Exist()
    {
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument>().AsQueryable().BuildMock());

        var command = new UpdateLegalDocumentCommand
        {
            LegalDocumentDto = new LegalDocumentDto { Id = Guid.NewGuid(), Title = "X", Content = "Y" }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Sözleşme bulunamadı");
    }
}
