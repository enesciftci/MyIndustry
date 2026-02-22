using MediatR;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.DeleteLegalDocumentCommand;

public class DeleteLegalDocumentCommand : IRequest<DeleteLegalDocumentCommandResult>
{
    public Guid Id { get; set; }
}
