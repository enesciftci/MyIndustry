using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.Contract.CreateContractCommand;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetActiveLegalDocumentsByTypesQuery;
using MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Domain.Provider;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.Contract;

public class ContractIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _legalDocumentRepository;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _sellerSubscriptionRepository;
    private readonly IGenericRepository<DomainSubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityProvider _securityProvider;

    public ContractIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _legalDocumentRepository = new GenericRepository<Domain.Aggregate.LegalDocument>(_context);
        _sellerRepository = new GenericRepository<Domain.Aggregate.Seller>(_context);
        _sellerSubscriptionRepository = new GenericRepository<Domain.Aggregate.SellerSubscription>(_context);
        _subscriptionPlanRepository = new GenericRepository<DomainSubscriptionPlan>(_context);
        _unitOfWork = new UnitOfWork(_context);
        _securityProvider = new SecurityProvider();
    }

    [Fact]
    public async Task CreateContractCommandHandler_Is_Not_Yet_Implemented()
    {
        var handler = new CreateContractCommandHandler();
        var act = () => handler.Handle(new CreateContractCommand(), CancellationToken.None);
        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task SellerAgreement_LegalDocument_Should_Be_Retrievable()
    {
        await TestDataBuilder.SeedLegalDocumentAsync(
            _context, LegalDocumentType.SellerAgreement, "Seller Agreement v1");

        var handler = new GetActiveLegalDocumentsByTypesQueryHandler(_legalDocumentRepository);
        var result = await handler.Handle(new GetActiveLegalDocumentsByTypesQuery
        {
            DocumentTypes = new List<int> { (int)LegalDocumentType.SellerAgreement }
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.LegalDocuments.Should().ContainSingle(d => d.Title == "Seller Agreement v1");
    }

    [Fact]
    public async Task CreateSeller_Should_Persist_Agreement_Url()
    {
        await TestDataBuilder.SeedFreePlanAsync(_context);
        var userId = Guid.NewGuid();
        const string agreementUrl = "https://example.com/contracts/seller-v1.pdf";

        var handler = new CreateSellerCommandHandler(
            _sellerRepository, _sellerSubscriptionRepository, _subscriptionPlanRepository,
            _unitOfWork, _securityProvider);

        var result = await handler.Handle(new CreateSellerCommand
        {
            UserId = userId,
            Title = "Contract Seller",
            Description = "Seller with agreement",
            IdentityNumber = "12345678901",
            Email = "contract@example.com",
            PhoneNumber = "+905551234567",
            AgreementUrl = agreementUrl,
            Sector = SellerSector.IronMongery
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var seller = await _context.Sellers.FindAsync(userId);
        seller!.AgreementUrl.Should().Be(agreementUrl);
    }

    [Fact]
    public async Task MembershipAgreement_Should_Be_In_Default_Active_Documents()
    {
        await TestDataBuilder.SeedLegalDocumentAsync(
            _context, LegalDocumentType.MembershipAgreement, "Membership Agreement");

        var handler = new GetActiveLegalDocumentsByTypesQueryHandler(_legalDocumentRepository);
        var result = await handler.Handle(new GetActiveLegalDocumentsByTypesQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.LegalDocuments.Should().Contain(d => d.Title == "Membership Agreement");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
