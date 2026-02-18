using MediatR;

namespace MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;

public record SendMessageCommand : IRequest<SendMessageCommandResult>
{
    public Guid ServiceId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string Content { get; set; }
}
