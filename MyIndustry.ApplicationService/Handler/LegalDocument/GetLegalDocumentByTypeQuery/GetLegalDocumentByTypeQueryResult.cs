using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByTypeQuery;

public record GetLegalDocumentByTypeQueryResult : ResponseBase
{
    public LegalDocumentDto LegalDocument { get; set; }
}
