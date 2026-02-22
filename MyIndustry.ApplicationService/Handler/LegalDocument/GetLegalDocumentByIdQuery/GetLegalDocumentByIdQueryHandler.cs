using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByIdQuery;

public class GetLegalDocumentByIdQueryHandler : IRequestHandler<GetLegalDocumentByIdQuery, GetLegalDocumentByIdQueryResult>
{
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _legalDocumentRepository;

    public GetLegalDocumentByIdQueryHandler(
        IGenericRepository<Domain.Aggregate.LegalDocument> legalDocumentRepository)
    {
        _legalDocumentRepository = legalDocumentRepository;
    }

    public async Task<GetLegalDocumentByIdQueryResult> Handle(GetLegalDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var legalDocument = await _legalDocumentRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (legalDocument == null)
        {
            return new GetLegalDocumentByIdQueryResult().ReturnNotFound("Sözleşme bulunamadı.");
        }

        var legalDocumentDto = new LegalDocumentDto
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

        return new GetLegalDocumentByIdQueryResult
        {
            LegalDocument = legalDocumentDto
        }.ReturnOk();
    }
}
