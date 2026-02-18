namespace MyIndustry.ApplicationService.Handler.Message.MarkMessagesAsReadCommand;

public record MarkMessagesAsReadCommandResult : ResponseBase
{
    public int MarkedCount { get; set; }
}
