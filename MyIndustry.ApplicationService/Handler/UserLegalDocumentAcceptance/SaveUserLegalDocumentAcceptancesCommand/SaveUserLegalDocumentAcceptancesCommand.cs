using MediatR;

namespace MyIndustry.ApplicationService.Handler.UserLegalDocumentAcceptance.SaveUserLegalDocumentAcceptancesCommand;

/// <summary>
/// Kayıt sonrası Identity servisi tarafından çağrılır; kullanıcının kabul ettiği sözleşmeleri kaydeder.
/// </summary>
public class SaveUserLegalDocumentAcceptancesCommand : IRequest<SaveUserLegalDocumentAcceptancesCommandResult>
{
    public Guid UserId { get; set; }
    public List<Guid> LegalDocumentIds { get; set; }
}
