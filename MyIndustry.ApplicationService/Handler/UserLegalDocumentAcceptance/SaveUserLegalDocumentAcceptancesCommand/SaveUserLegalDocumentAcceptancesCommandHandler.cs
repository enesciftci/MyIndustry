using MediatR;
using MyIndustry.Repository.Repository;

namespace MyIndustry.ApplicationService.Handler.UserLegalDocumentAcceptance.SaveUserLegalDocumentAcceptancesCommand;

public class SaveUserLegalDocumentAcceptancesCommandHandler : IRequestHandler<SaveUserLegalDocumentAcceptancesCommand, SaveUserLegalDocumentAcceptancesCommandResult>
{
    private readonly IGenericRepository<MyIndustry.Domain.Aggregate.UserLegalDocumentAcceptance> _acceptanceRepository;

    public SaveUserLegalDocumentAcceptancesCommandHandler(
        IGenericRepository<MyIndustry.Domain.Aggregate.UserLegalDocumentAcceptance> acceptanceRepository)
    {
        _acceptanceRepository = acceptanceRepository;
    }

    public async Task<SaveUserLegalDocumentAcceptancesCommandResult> Handle(SaveUserLegalDocumentAcceptancesCommand request, CancellationToken cancellationToken)
    {
        if (request.LegalDocumentIds == null || !request.LegalDocumentIds.Any())
            return new SaveUserLegalDocumentAcceptancesCommandResult().ReturnOk();

        var acceptedAt = DateTime.UtcNow;
        foreach (var docId in request.LegalDocumentIds.Distinct())
        {
            var acceptance = new MyIndustry.Domain.Aggregate.UserLegalDocumentAcceptance
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                LegalDocumentId = docId,
                AcceptedAt = acceptedAt,
                CreatedDate = acceptedAt
            };
            await _acceptanceRepository.AddAsync(acceptance, cancellationToken);
        }

        return new SaveUserLegalDocumentAcceptancesCommandResult().ReturnOk();
    }
}
