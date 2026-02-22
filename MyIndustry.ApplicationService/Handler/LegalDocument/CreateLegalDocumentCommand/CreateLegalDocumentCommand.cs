using MediatR;
using MyIndustry.ApplicationService.Dto;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.CreateLegalDocumentCommand;

public class CreateLegalDocumentCommand : IRequest<CreateLegalDocumentCommandResult>
{
    public LegalDocumentDto LegalDocumentDto { get; set; }
}
