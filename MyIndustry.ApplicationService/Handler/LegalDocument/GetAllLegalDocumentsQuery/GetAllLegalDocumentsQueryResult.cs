using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetAllLegalDocumentsQuery;

public record GetAllLegalDocumentsQueryResult : ResponseBase
{
    public List<LegalDocumentDto> LegalDocuments { get; set; }
}
