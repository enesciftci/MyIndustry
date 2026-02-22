using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.DeleteLegalDocumentCommand;

public class DeleteLegalDocumentCommandHandler : IRequestHandler<DeleteLegalDocumentCommand, DeleteLegalDocumentCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _legalDocumentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLegalDocumentCommandHandler(
        IGenericRepository<Domain.Aggregate.LegalDocument> legalDocumentRepository,
        IUnitOfWork unitOfWork)
    {
        _legalDocumentRepository = legalDocumentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteLegalDocumentCommandResult> Handle(DeleteLegalDocumentCommand request, CancellationToken cancellationToken)
    {
        var legalDocument = await _legalDocumentRepository.GetAllQuery()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (legalDocument == null)
        {
            return new DeleteLegalDocumentCommandResult().ReturnNotFound("Sözleşme bulunamadı.");
        }

        _legalDocumentRepository.Delete(legalDocument);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteLegalDocumentCommandResult().ReturnOk("Sözleşme başarıyla silindi.");
    }
}
