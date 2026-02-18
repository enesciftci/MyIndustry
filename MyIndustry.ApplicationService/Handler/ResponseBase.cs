using System.Text.Json.Serialization;

namespace MyIndustry.ApplicationService.Handler;

public record ResponseBase
{
    public bool Success { get; set; }
    public string MessageCode { get; set; }
    public string Message { get; set; }
    public string UserMessage { get; set; }

    [JsonIgnore]
    public Dictionary<string, string> LogParameters { get; set; }
    public bool IsTimeout()
    {
        return MessageCode == "1907";
    }
}

public record ResponseBase<TResponse> : ResponseBase where TResponse : class
{
    public TResponse Result { get; set; }
}

public static class ResponseBaseExtensions
{
    public static TResponse ReturnOk<TResponse>(this TResponse responseBase) where TResponse : ResponseBase
    {
        responseBase.Success = true;
        responseBase.MessageCode = "0000";
        responseBase.Message = "İşlem başarıyla gerçekleştirildi.";
        responseBase.UserMessage = "İşlem başarıyla gerçekleştirildi.";

        return responseBase;
    }

    public static TResponse ReturnBad<TResponse>(this TResponse responseBase) where TResponse : ResponseBase
    {
        responseBase.Success = false;
        responseBase.MessageCode = "1001";
        responseBase.Message = "Bir hata oluştu";
        responseBase.UserMessage = "Bir hata oluştu";

        return responseBase;
    }

    public static TResponse ReturnBadRequest<TResponse>(this TResponse responseBase, string message) where TResponse : ResponseBase
    {
        responseBase.Success = false;
        responseBase.MessageCode = "1001";
        responseBase.Message = message;
        responseBase.UserMessage = message;

        return responseBase;
    }

    public static TResponse ReturnNotFound<TResponse>(this TResponse responseBase, string message) where TResponse : ResponseBase
    {
        responseBase.Success = false;
        responseBase.MessageCode = "1004";
        responseBase.Message = message;
        responseBase.UserMessage = message;

        return responseBase;
    }

    public static TResponse ReturnOk<TResponse>(this TResponse responseBase, string message) where TResponse : ResponseBase
    {
        responseBase.Success = true;
        responseBase.MessageCode = "0000";
        responseBase.Message = message;
        responseBase.UserMessage = message;

        return responseBase;
    }

    public static TResponse Return<TResponse>(this TResponse responseBase, bool success, string messageCode,
        string message, string userMessage, Dictionary<string, string> logParameters = null)
        where TResponse : ResponseBase
    {
        responseBase.Success = success;
        responseBase.MessageCode = messageCode;
        responseBase.Message = message;
        responseBase.UserMessage = userMessage;

        return responseBase;
    }
}
