using MediatR;

namespace MyIndustry.ApplicationService.Handler.Verification.SendEmailChangeVerificationCommand;

public sealed record SendEmailChangeVerificationCommand : IRequest<SendEmailChangeVerificationCommandResult>
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; }
}
