using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByTypeQuery;

public class GetLegalDocumentByTypeQueryHandler : IRequestHandler<GetLegalDocumentByTypeQuery, GetLegalDocumentByTypeQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _legalDocumentRepository;

    public GetLegalDocumentByTypeQueryHandler(
        IGenericRepository<Domain.Aggregate.LegalDocument> legalDocumentRepository)
    {
        _legalDocumentRepository = legalDocumentRepository;
    }

    public async Task<GetLegalDocumentByTypeQueryResult> Handle(GetLegalDocumentByTypeQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var legalDocument = await _legalDocumentRepository
            .GetAllQuery()
            .Where(l => l.IsActive && (int)l.DocumentType == request.DocumentType
                && (l.EffectiveDate == null || l.EffectiveDate <= now)
                && (l.ExpiryDate == null || l.ExpiryDate > now))
            .OrderByDescending(l => l.EffectiveDate)
            .ThenByDescending(l => l.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (legalDocument == null)
        {
            return new GetLegalDocumentByTypeQueryResult().ReturnNotFound("Sözleşme bulunamadı.");
        }

        var dto = new LegalDocumentDto
        {
            Id = legalDocument.Id,
            DocumentType = legalDocument.DocumentType,
            Title = legalDocument.Title,
            Content = legalDocument.Content,
            Version = legalDocument.Version,
            IsActive = legalDocument.IsActive,
            EffectiveDate = legalDocument.EffectiveDate,
            ExpiryDate = legalDocument.ExpiryDate,
            DisplayOrder = legalDocument.DisplayOrder,
            CreatedDate = legalDocument.CreatedDate,
            ModifiedDate = legalDocument.ModifiedDate
        };

        return new GetLegalDocumentByTypeQueryResult { LegalDocument = dto }.ReturnOk();
    }
}
