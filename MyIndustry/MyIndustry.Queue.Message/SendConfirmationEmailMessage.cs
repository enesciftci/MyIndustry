namespace MyIndustry.Queue.Message;

public sealed record SendConfirmationEmailMessage : BaseMessage
{
    public string Subject { get; set; }
    public string Body { get; set; }
}