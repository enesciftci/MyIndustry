using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByIdQuery;

public record GetLegalDocumentByIdQueryResult : ResponseBase
{
    public LegalDocumentDto LegalDocument { get; set; }
}
