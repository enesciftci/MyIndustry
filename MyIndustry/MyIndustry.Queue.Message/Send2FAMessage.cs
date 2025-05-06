namespace MyIndustry.Queue.Message;

public sealed record Send2FAMessage : BaseMessage
{
    public string Code { get; set; }
}