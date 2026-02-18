using MediatR;

namespace MyIndustry.ApplicationService.Handler.Message.ReplyMessageCommand;

public record ReplyMessageCommand : IRequest<ReplyMessageCommandResult>
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public Guid ServiceId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; }
}
