using MediatR;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetActiveLegalDocumentsByTypesQuery;

/// <summary>
/// Kayıt ve genel kullanım için aktif sözleşmeleri tip listesine göre getirir (herkese açık).
/// </summary>
public class GetActiveLegalDocumentsByTypesQuery : IRequest<GetActiveLegalDocumentsByTypesQueryResult>
{
    /// <summary>
    /// LegalDocumentType enum değerleri. Boş ise kayıt için kullanılan tipler (KVKK, Üyelik, Kullanım Şartları, Gizlilik) döner.
    /// </summary>
    public List<int> DocumentTypes { get; set; }
}
