namespace MyIndustry.ApplicationService.Handler.Message.ReplyMessageCommand;

public record ReplyMessageCommandResult : ResponseBase
{
    public Guid MessageId { get; set; }
}
