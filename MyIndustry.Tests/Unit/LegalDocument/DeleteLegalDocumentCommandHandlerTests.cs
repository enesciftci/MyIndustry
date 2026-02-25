using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.LegalDocument.DeleteLegalDocumentCommand;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.Tests.Unit.LegalDocument;

public class DeleteLegalDocumentCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainLegalDocument>> _legalDocumentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteLegalDocumentCommandHandler _handler;

    public DeleteLegalDocumentCommandHandlerTests()
    {
        _legalDocumentRepositoryMock = new Mock<IGenericRepository<DomainLegalDocument>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteLegalDocumentCommandHandler(_legalDocumentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_When_Document_Exists()
    {
        var id = Guid.NewGuid();
        var doc = new DomainLegalDocument { Id = id, Title = "Silinecek", Content = "İçerik" };
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument> { doc }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new DeleteLegalDocumentCommand { Id = id }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("silindi");
        _legalDocumentRepositoryMock.Verify(r => r.Delete(doc), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Document_Does_Not_Exist()
    {
        _legalDocumentRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainLegalDocument>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new DeleteLegalDocumentCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Sözleşme bulunamadı");
    }
}
