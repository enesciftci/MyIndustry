namespace MyIndustry.Queue.Message;

public class SendVerificationCodeMessage : BaseMessage
{
    public string Code { get; set; }
}