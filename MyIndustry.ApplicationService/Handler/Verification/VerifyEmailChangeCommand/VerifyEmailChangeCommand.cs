using MediatR;

namespace MyIndustry.ApplicationService.Handler.Verification.VerifyEmailChangeCommand;

public sealed record VerifyEmailChangeCommand : IRequest<VerifyEmailChangeCommandResult>
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; }
    public string Code { get; set; }
}
