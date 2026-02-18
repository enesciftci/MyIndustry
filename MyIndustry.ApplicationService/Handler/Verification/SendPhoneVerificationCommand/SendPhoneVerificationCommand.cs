using MediatR;

namespace MyIndustry.ApplicationService.Handler.Verification.SendPhoneVerificationCommand;

public sealed record SendPhoneVerificationCommand : IRequest<SendPhoneVerificationCommandResult>
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; }
}
