using MediatR;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.UpdateLegalDocumentCommand;

public class UpdateLegalDocumentCommand : IRequest<UpdateLegalDocumentCommandResult>
{
    public LegalDocumentDto LegalDocumentDto { get; set; }
}
