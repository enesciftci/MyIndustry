using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.UserLegalDocumentAcceptance.SaveUserLegalDocumentAcceptancesCommand;
using MyIndustry.Repository.Repository;
using DomainAcceptance = MyIndustry.Domain.Aggregate.UserLegalDocumentAcceptance;

namespace MyIndustry.Tests.Unit.UserLegalDocumentAcceptance;

public class SaveUserLegalDocumentAcceptancesCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainAcceptance>> _acceptanceRepositoryMock;
    private readonly SaveUserLegalDocumentAcceptancesCommandHandler _handler;

    public SaveUserLegalDocumentAcceptancesCommandHandlerTests()
    {
        _acceptanceRepositoryMock = new Mock<IGenericRepository<DomainAcceptance>>();
        _handler = new SaveUserLegalDocumentAcceptancesCommandHandler(_acceptanceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Ok_Without_Add_When_LegalDocumentIds_Null()
    {
        var result = await _handler.Handle(
            new SaveUserLegalDocumentAcceptancesCommand { UserId = Guid.NewGuid(), LegalDocumentIds = null! },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        _acceptanceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainAcceptance>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Ok_Without_Add_When_LegalDocumentIds_Empty()
    {
        var result = await _handler.Handle(
            new SaveUserLegalDocumentAcceptancesCommand { UserId = Guid.NewGuid(), LegalDocumentIds = new List<Guid>() },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        _acceptanceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainAcceptance>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Add_Acceptances_For_Each_DocumentId()
    {
        var userId = Guid.NewGuid();
        var docId1 = Guid.NewGuid();
        var docId2 = Guid.NewGuid();
        _acceptanceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainAcceptance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new SaveUserLegalDocumentAcceptancesCommand { UserId = userId, LegalDocumentIds = new List<Guid> { docId1, docId2 } },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        _acceptanceRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainAcceptance>(a =>
            a.UserId == userId && (a.LegalDocumentId == docId1 || a.LegalDocumentId == docId2)
        ), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_Should_Add_Once_Per_Distinct_DocumentId_When_Ids_Repeated()
    {
        var userId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        _acceptanceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainAcceptance>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new SaveUserLegalDocumentAcceptancesCommand { UserId = userId, LegalDocumentIds = new List<Guid> { docId, docId, docId } },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        _acceptanceRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainAcceptance>(a =>
            a.UserId == userId && a.LegalDocumentId == docId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
