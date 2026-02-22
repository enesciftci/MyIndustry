using MediatR;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainLegalDocument = MyIndustry.Domain.Aggregate.LegalDocument;

namespace MyIndustry.ApplicationService.Handler.LegalDocument.CreateLegalDocumentCommand;

public class CreateLegalDocumentCommandHandler : IRequestHandler<CreateLegalDocumentCommand, CreateLegalDocumentCommandResult>
{
    private readonly IGenericRepository<DomainLegalDocument> _legalDocumentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLegalDocumentCommandHandler(
        IGenericRepository<DomainLegalDocument> legalDocumentRepository,
        IUnitOfWork unitOfWork)
    {
        _legalDocumentRepository = legalDocumentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateLegalDocumentCommandResult> Handle(CreateLegalDocumentCommand request, CancellationToken cancellationToken)
    {
        var legalDocument = new DomainLegalDocument
        {
            DocumentType = request.LegalDocumentDto.DocumentType,
            Title = request.LegalDocumentDto.Title,
            Content = request.LegalDocumentDto.Content,
            Version = request.LegalDocumentDto.Version,
            IsActive = request.LegalDocumentDto.IsActive,
            EffectiveDate = request.LegalDocumentDto.EffectiveDate,
            ExpiryDate = request.LegalDocumentDto.ExpiryDate,
            DisplayOrder = request.LegalDocumentDto.DisplayOrder,
            CreatedDate = DateTime.UtcNow
        };

        await _legalDocumentRepository.AddAsync(legalDocument, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateLegalDocumentCommandResult().ReturnOk("Sözleşme başarıyla oluşturuldu.");
    }
}
