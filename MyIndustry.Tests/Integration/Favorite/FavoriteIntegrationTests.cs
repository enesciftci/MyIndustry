using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.Favorite.AddFavoriteCommand;
using MyIndustry.ApplicationService.Handler.Favorite.DeleteFavoriteCommand;
using MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteListQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.Favorite;

public class FavoriteIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<Domain.Aggregate.Favorite> _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _favoriteRepository = new GenericRepository<Domain.Aggregate.Favorite>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task AddFavorite_Should_Save_To_Database()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);
        var userId = Guid.NewGuid();

        var handler = new AddFavoriteCommandHandler(_unitOfWork, _favoriteRepository);
        var result = await handler.Handle(new AddFavoriteCommand
        {
            UserId = userId,
            ServiceId = service.Id
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceId == service.Id);
        favorite.Should().NotBeNull();
    }

    [Fact]
    public async Task AddFavorite_Duplicate_Should_Be_Idempotent()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);
        var userId = Guid.NewGuid();

        var handler = new AddFavoriteCommandHandler(_unitOfWork, _favoriteRepository);
        var command = new AddFavoriteCommand { UserId = userId, ServiceId = service.Id };

        await handler.Handle(command, CancellationToken.None);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        var count = await _context.Favorites
            .CountAsync(f => f.UserId == userId && f.ServiceId == service.Id);
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetFavoriteList_Should_Return_User_Favorites()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Favorited Service");
        var userId = Guid.NewGuid();

        _context.Favorites.Add(new Domain.Aggregate.Favorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ServiceId = service.Id,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new GetFavoriteListQueryHandler(_favoriteRepository);
        var result = await handler.Handle(new GetFavoriteListQuery { UserId = userId }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Favorites.Should().HaveCount(1);
        result.Favorites[0].Service.Title.Should().Be("Favorited Service");
        result.Favorites[0].IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFavorite_Should_Remove_From_Database()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);
        var userId = Guid.NewGuid();
        var favoriteId = Guid.NewGuid();

        _context.Favorites.Add(new Domain.Aggregate.Favorite
        {
            Id = favoriteId,
            UserId = userId,
            ServiceId = service.Id,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new DeleteFavoriteCommandHandler(_unitOfWork, _favoriteRepository);
        var result = await handler.Handle(new DeleteFavoriteCommand
        {
            UserId = userId,
            FavoriteId = favoriteId,
            ServiceId = service.Id
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var deleted = await _context.Favorites.FindAsync(favoriteId);
        deleted.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
