using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Message.GetConversationsQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.Tests.Unit.Message;

public class GetConversationsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainMessage>> _messageRepositoryMock;
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly GetConversationsQueryHandler _handler;

    public GetConversationsQueryHandlerTests()
    {
        _messageRepositoryMock = new Mock<IGenericRepository<DomainMessage>>();
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _handler = new GetConversationsQueryHandler(
            _messageRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _sellerRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Conversations_When_No_Messages()
    {
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetConversationsQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Conversations.Should().NotBeNull().And.BeEmpty();
    }
}
