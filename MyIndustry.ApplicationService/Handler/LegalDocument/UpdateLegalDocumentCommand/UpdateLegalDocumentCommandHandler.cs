using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.UpdateLegalDocumentCommand;

public class UpdateLegalDocumentCommandHandler : IRequestHandler<UpdateLegalDocumentCommand, UpdateLegalDocumentCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.LegalDocument> _legalDocumentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLegalDocumentCommandHandler(
        IGenericRepository<Domain.Aggregate.LegalDocument> legalDocumentRepository,
        IUnitOfWork unitOfWork)
    {
        _legalDocumentRepository = legalDocumentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateLegalDocumentCommandResult> Handle(UpdateLegalDocumentCommand request, CancellationToken cancellationToken)
    {
        var legalDocument = await _legalDocumentRepository.GetAllQuery()
            .FirstOrDefaultAsync(l => l.Id == request.LegalDocumentDto.Id, cancellationToken);

        if (legalDocument == null)
        {
            return new UpdateLegalDocumentCommandResult().ReturnNotFound("Sözleşme bulunamadı.");
        }

        legalDocument.DocumentType = request.LegalDocumentDto.DocumentType;
        legalDocument.Title = request.LegalDocumentDto.Title;
        legalDocument.Content = request.LegalDocumentDto.Content;
        legalDocument.Version = request.LegalDocumentDto.Version;
        legalDocument.IsActive = request.LegalDocumentDto.IsActive;
        legalDocument.EffectiveDate = request.LegalDocumentDto.EffectiveDate;
        legalDocument.ExpiryDate = request.LegalDocumentDto.ExpiryDate;
        legalDocument.DisplayOrder = request.LegalDocumentDto.DisplayOrder;
        legalDocument.ModifiedDate = DateTime.UtcNow;

        _legalDocumentRepository.Update(legalDocument);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateLegalDocumentCommandResult().ReturnOk("Sözleşme başarıyla güncellendi.");
    }
}
