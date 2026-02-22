using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.CreateSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.DeleteSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetAllSubscriptionPlansQuery;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.UpdateSubscriptionPlanCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.SubscriptionPlan;

public class SubscriptionPlanIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<DomainSubscriptionPlan> _repository;
    private readonly IGenericRepository<SellerSubscription> _sellerSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionPlanIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _repository = new GenericRepository<DomainSubscriptionPlan>(_context);
        _sellerSubscriptionRepository = new GenericRepository<SellerSubscription>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task CreateSubscriptionPlan_Should_Save_To_Database()
    {
        // Arrange
        var handler = new CreateSubscriptionPlanCommandHandler(_repository, _unitOfWork);
        var command = new CreateSubscriptionPlanCommand
        {
            Name = "Integration Test Plan",
            Description = "Test Description",
            SubscriptionType = SubscriptionType.Premium,
            MonthlyPrice = 59900,
            MonthlyPostLimit = 50,
            PostDurationInDays = 60,
            FeaturedPostLimit = 10
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var savedPlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == command.Name);
        savedPlan.Should().NotBeNull();
        savedPlan!.Name.Should().Be(command.Name);
        savedPlan.Description.Should().Be(command.Description);
        savedPlan.SubscriptionType.Should().Be(command.SubscriptionType);
        savedPlan.MonthlyPrice.Should().Be(command.MonthlyPrice);
        savedPlan.MonthlyPostLimit.Should().Be(command.MonthlyPostLimit);
        savedPlan.PostDurationInDays.Should().Be(command.PostDurationInDays);
        savedPlan.FeaturedPostLimit.Should().Be(command.FeaturedPostLimit);
        savedPlan.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateSubscriptionPlan_Should_Update_In_Database()
    {
        // Arrange
        var existingPlan = new DomainSubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Original Plan",
            Description = "Original Description",
            SubscriptionType = SubscriptionType.Standard,
            MonthlyPrice = 29900,
            MonthlyPostLimit = 15,
            PostDurationInDays = 45,
            FeaturedPostLimit = 2,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _context.SubscriptionPlans.AddAsync(existingPlan);
        await _context.SaveChangesAsync();

        var handler = new UpdateSubscriptionPlanCommandHandler(_repository, _unitOfWork);
        var command = new UpdateSubscriptionPlanCommand
        {
            Id = existingPlan.Id,
            Name = "Updated Plan",
            Description = "Updated Description",
            SubscriptionType = SubscriptionType.Premium,
            MonthlyPrice = 59900,
            MonthlyPostLimit = 50,
            PostDurationInDays = 60,
            FeaturedPostLimit = 10,
            IsActive = false
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var updatedPlan = await _context.SubscriptionPlans.FindAsync(existingPlan.Id);
        updatedPlan.Should().NotBeNull();
        updatedPlan!.Name.Should().Be(command.Name);
        updatedPlan.Description.Should().Be(command.Description);
        updatedPlan.SubscriptionType.Should().Be(command.SubscriptionType);
        updatedPlan.MonthlyPrice.Should().Be(command.MonthlyPrice);
        updatedPlan.MonthlyPostLimit.Should().Be(command.MonthlyPostLimit);
        updatedPlan.PostDurationInDays.Should().Be(command.PostDurationInDays);
        updatedPlan.FeaturedPostLimit.Should().Be(command.FeaturedPostLimit);
        updatedPlan.IsActive.Should().Be(command.IsActive);
        updatedPlan.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllSubscriptionPlans_Should_Return_All_Plans()
    {
        // Arrange
        var plans = new List<DomainSubscriptionPlan>
        {
            new DomainSubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Plan 1",
                Description = "Description 1",
                SubscriptionType = SubscriptionType.Free,
                MonthlyPrice = 0,
                MonthlyPostLimit = 3,
                PostDurationInDays = 30,
                FeaturedPostLimit = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new DomainSubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Plan 2",
                Description = "Description 2",
                SubscriptionType = SubscriptionType.Premium,
                MonthlyPrice = 59900,
                MonthlyPostLimit = 50,
                PostDurationInDays = 60,
                FeaturedPostLimit = 10,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };

        await _context.SubscriptionPlans.AddRangeAsync(plans);
        await _context.SaveChangesAsync();

        var handler = new GetAllSubscriptionPlansQueryHandler(_repository);

        // Act
        var result = await handler.Handle(new GetAllSubscriptionPlansQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Plans.Should().HaveCount(2);
        result.Plans.Should().Contain(p => p.Name == "Plan 1");
        result.Plans.Should().Contain(p => p.Name == "Plan 2");
    }

    [Fact]
    public async Task DeleteSubscriptionPlan_Should_Remove_From_Database()
    {
        // Arrange
        var plan = new DomainSubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Plan To Delete",
            Description = "Description",
            SubscriptionType = SubscriptionType.Standard,
            MonthlyPrice = 29900,
            MonthlyPostLimit = 15,
            PostDurationInDays = 45,
            FeaturedPostLimit = 2,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _context.SubscriptionPlans.AddAsync(plan);
        await _context.SaveChangesAsync();

        var handler = new DeleteSubscriptionPlanCommandHandler(_repository, _sellerSubscriptionRepository, _unitOfWork);
        var command = new DeleteSubscriptionPlanCommand { Id = plan.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var deletedPlan = await _context.SubscriptionPlans.FindAsync(plan.Id);
        deletedPlan.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
