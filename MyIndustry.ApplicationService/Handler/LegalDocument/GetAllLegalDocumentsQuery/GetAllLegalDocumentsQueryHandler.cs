using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetAllLegalDocumentsQuery;

public class GetAllLegalDocumentsQueryHandler : IRequestHandler<GetAllLegalDocumentsQuery, GetAllLegalDocumentsQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _legalDocumentRepository;

    public GetAllLegalDocumentsQueryHandler(
        IGenericRepository<Domain.Aggregate.LegalDocument> legalDocumentRepository)
    {
        _legalDocumentRepository = legalDocumentRepository;
    }

    public async Task<GetAllLegalDocumentsQueryResult> Handle(GetAllLegalDocumentsQuery request, CancellationToken cancellationToken)
    {
        var legalDocuments = await _legalDocumentRepository
            .GetAllQuery()
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

        return new GetAllLegalDocumentsQueryResult
        {
            LegalDocuments = legalDocuments
        }.ReturnOk();
    }
}
