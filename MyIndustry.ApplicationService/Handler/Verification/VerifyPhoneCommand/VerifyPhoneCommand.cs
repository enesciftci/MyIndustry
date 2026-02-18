using MediatR;

namespace MyIndustry.ApplicationService.Handler.Verification.VerifyPhoneCommand;

public sealed record VerifyPhoneCommand : IRequest<VerifyPhoneCommandResult>
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; }
    public string Code { get; set; }
}
