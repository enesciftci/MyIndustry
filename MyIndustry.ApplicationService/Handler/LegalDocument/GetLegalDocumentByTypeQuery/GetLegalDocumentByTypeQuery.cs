using MediatR;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByTypeQuery;

/// <summary>
/// Tek bir sözleşme tipine göre aktif sözleşmeyi getirir (örn. /terms, /privacy sayfaları için).
/// </summary>
public class GetLegalDocumentByTypeQuery : IRequest<GetLegalDocumentByTypeQueryResult>
{
    public int DocumentType { get; set; }
}
