namespace MyIndustry.Domain.ExceptionHandling;

public class BusinessRuleException : Exception
{
    public string Code { get; set; }

    public new string Message { get; set; }
    
    public string UserMessage { get; set; }

    public BusinessRuleException()
    {
    }

    public BusinessRuleException(string code, string message, string userMessage)
        : base(message)
    {
        this.Code = code;
        this.Message = message;
        this.UserMessage = userMessage;
    }

    public BusinessRuleException(string message)
        : base(message)
    {
    }
}