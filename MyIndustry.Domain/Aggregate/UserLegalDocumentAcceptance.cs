namespace MyIndustry.Domain.Aggregate;

/// <summary>
/// Kayıt sırasında kullanıcının kabul ettiği sözleşmeleri tutar.
/// UserId: Identity servisindeki kullanıcı Id'si.
/// </summary>
public class UserLegalDocumentAcceptance : Entity
{
    public Guid UserId { get; set; }
    public Guid LegalDocumentId { get; set; }
    public DateTime AcceptedAt { get; set; }
}
