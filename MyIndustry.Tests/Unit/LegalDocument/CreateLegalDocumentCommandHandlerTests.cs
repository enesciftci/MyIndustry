using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.LegalDocument.CreateLegalDocumentCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.Tests.Unit.LegalDocument;

public class CreateLegalDocumentCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainLegalDocument>> _legalDocumentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateLegalDocumentCommandHandler _handler;

    public CreateLegalDocumentCommandHandlerTests()
    {
        _legalDocumentRepositoryMock = new Mock<IGenericRepository<DomainLegalDocument>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateLegalDocumentCommandHandler(_legalDocumentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_LegalDocument_Successfully()
    {
        _legalDocumentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainLegalDocument>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var dto = new LegalDocumentDto
        {
            DocumentType = LegalDocumentType.PrivacyPolicy,
            Title = "Gizlilik Politikası",
            Content = "İçerik",
            Version = "1.0",
            IsActive = true,
            EffectiveDate = DateTime.UtcNow,
            DisplayOrder = 1
        };
        var command = new CreateLegalDocumentCommand { LegalDocumentDto = dto };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _legalDocumentRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainLegalDocument>(d =>
            d.DocumentType == LegalDocumentType.PrivacyPolicy &&
            d.Title == "Gizlilik Politikası" &&
            d.Content == "İçerik" &&
            d.IsActive
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
