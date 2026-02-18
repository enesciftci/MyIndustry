using MediatR;

namespace MyIndustry.ApplicationService.Handler.Message.MarkMessagesAsReadCommand;

public record MarkMessagesAsReadCommand : IRequest<MarkMessagesAsReadCommandResult>
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid OtherUserId { get; set; }
}
