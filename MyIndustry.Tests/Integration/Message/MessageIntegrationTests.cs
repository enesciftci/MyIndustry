using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.Message.GetConversationMessagesQuery;
using MyIndustry.ApplicationService.Handler.Message.GetConversationsQuery;
using MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;
using MyIndustry.Domain.Aggregate;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.Message;

public class MessageIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<Domain.Aggregate.Message> _messageRepository;
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<DomainSeller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MessageIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _messageRepository = new GenericRepository<Domain.Aggregate.Message>(_context);
        _serviceRepository = new GenericRepository<Domain.Aggregate.Service>(_context);
        _sellerRepository = new GenericRepository<DomainSeller>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task SendMessage_Should_Save_Message_To_Database()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);
        var buyerId = Guid.NewGuid();

        var handler = new SendMessageCommandHandler(_messageRepository, _serviceRepository, _unitOfWork);
        var result = await handler.Handle(new SendMessageCommand
        {
            ServiceId = service.Id,
            SenderId = buyerId,
            SenderName = "Buyer User",
            SenderEmail = "buyer@example.com",
            Content = "Is this machine still available?"
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.MessageId.Should().NotBeEmpty();
        var saved = await _context.Messages.FindAsync(result.MessageId);
        saved.Should().NotBeNull();
        saved!.ReceiverId.Should().Be(seller.Id);
        saved.Content.Should().Be("Is this machine still available?");
        saved.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessage_To_Own_Listing_Should_Return_BadRequest()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);

        var handler = new SendMessageCommandHandler(_messageRepository, _serviceRepository, _unitOfWork);
        var result = await handler.Handle(new SendMessageCommand
        {
            ServiceId = service.Id,
            SenderId = seller.Id,
            SenderName = "Seller",
            SenderEmail = "seller@example.com",
            Content = "Self message"
        }, CancellationToken.None);

        result.Success.Should().BeFalse();
        var count = await _context.Messages.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetConversations_Should_Group_Messages_By_Service()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Message Service");
        var buyerId = Guid.NewGuid();

        _context.Messages.AddRange(
            new Domain.Aggregate.Message
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                SenderId = buyerId,
                ReceiverId = seller.Id,
                SenderName = "Buyer",
                SenderEmail = "buyer@example.com",
                Content = "First message",
                IsRead = false,
                CreatedDate = DateTime.UtcNow.AddMinutes(-5)
            },
            new Domain.Aggregate.Message
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                SenderId = buyerId,
                ReceiverId = seller.Id,
                SenderName = "Buyer",
                SenderEmail = "buyer@example.com",
                Content = "Second message",
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });
        await _context.SaveChangesAsync();

        var handler = new GetConversationsQueryHandler(_messageRepository, _serviceRepository, _sellerRepository);
        var result = await handler.Handle(new GetConversationsQuery { UserId = seller.Id }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Conversations.Should().HaveCount(1);
        result.Conversations[0].ServiceTitle.Should().Be("Message Service");
        result.Conversations[0].UnreadCount.Should().Be(2);
    }

    [Fact]
    public async Task GetConversationMessages_Should_Return_Thread()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);
        var buyerId = Guid.NewGuid();

        _context.Messages.Add(new Domain.Aggregate.Message
        {
            Id = Guid.NewGuid(),
            ServiceId = service.Id,
            SenderId = buyerId,
            ReceiverId = seller.Id,
            SenderName = "Buyer",
            SenderEmail = "buyer@example.com",
            Content = "Thread message",
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new GetConversationMessagesQueryHandler(_messageRepository, _serviceRepository);
        var result = await handler.Handle(new GetConversationMessagesQuery
        {
            UserId = seller.Id,
            OtherUserId = buyerId,
            ServiceId = service.Id
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Content.Should().Be("Thread message");
        result.ServiceTitle.Should().Be(service.Title);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
