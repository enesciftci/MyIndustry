using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetActiveLegalDocumentsByTypesQuery;

public record GetActiveLegalDocumentsByTypesQueryResult : ResponseBase
{
    public List<LegalDocumentDto> LegalDocuments { get; set; }
}
