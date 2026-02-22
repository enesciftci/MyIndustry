using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetActiveLegalDocumentsByTypesQuery;

public class GetActiveLegalDocumentsByTypesQueryHandler : IRequestHandler<GetActiveLegalDocumentsByTypesQuery, GetActiveLegalDocumentsByTypesQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _legalDocumentRepository;

    public GetActiveLegalDocumentsByTypesQueryHandler(
        IGenericRepository<Domain.Aggregate.LegalDocument> legalDocumentRepository)
    {
        _legalDocumentRepository = legalDocumentRepository;
    }

    public async Task<GetActiveLegalDocumentsByTypesQueryResult> Handle(GetActiveLegalDocumentsByTypesQuery request, CancellationToken cancellationToken)
    {
        var types = request.DocumentTypes?.Any() == true
            ? request.DocumentTypes
            : new List<int>
            {
                (int)LegalDocumentType.KVKK,
                (int)LegalDocumentType.MembershipAgreement,
                (int)LegalDocumentType.TermsOfService,
                (int)LegalDocumentType.PrivacyPolicy
            };

        var now = DateTime.UtcNow;
        var documents = await _legalDocumentRepository
            .GetAllQuery()
            .Where(l => l.IsActive && types.Contains((int)l.DocumentType)
                && (l.EffectiveDate == null || l.EffectiveDate <= now)
                && (l.ExpiryDate == null || l.ExpiryDate > now))
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.CreatedDate)
            .Select(l => new LegalDocumentDto
            {
                Id = l.Id,
                DocumentType = l.DocumentType,
                Title = l.Title,
                Content = l.Content,
                Version = l.Version,
                IsActive = l.IsActive,
                EffectiveDate = l.EffectiveDate,
                ExpiryDate = l.ExpiryDate,
                DisplayOrder = l.DisplayOrder,
                CreatedDate = l.CreatedDate,
                ModifiedDate = l.ModifiedDate
            })
            .ToListAsync(cancellationToken);

        return new GetActiveLegalDocumentsByTypesQueryResult
        {
            LegalDocuments = documents
        }.ReturnOk();
    }
}
