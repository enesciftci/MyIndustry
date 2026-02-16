namespace MyIndustry.Queue.Message;

public record SendForgotPasswordEmailMessage()
{
    public string Email { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}