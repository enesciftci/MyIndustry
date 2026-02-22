using MediatR;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByIdQuery;

public class GetLegalDocumentByIdQuery : IRequest<GetLegalDocumentByIdQueryResult>
{
    public Guid Id { get; set; }
}
