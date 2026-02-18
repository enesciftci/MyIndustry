namespace MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;

public record SendMessageCommandResult : ResponseBase
{
    public Guid MessageId { get; set; }
}
