using MediatR;
using MyIndustry.Container.Logging;

namespace MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;

public record SendMessageCommand : IRequest<SendMessageCommandResult>
{
    public Guid ServiceId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }

    [LogSensitive(Mode = LogMaskMode.Partial)]
    public string SenderEmail { get; set; }

    [LogSensitive(Mode = LogMaskMode.Truncate, MaxLength = 100)]
    public string Content { get; set; }
}
