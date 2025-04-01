using System.Text.Json.Serialization;

namespace CoreApiCommunicator.Response;

public class ResponseBase
{
    public bool Success { get; set; }
    public string MessageCode { get; set; }
    public string Message { get; set; }
    public string UserMessage { get; set; }
    public bool IsTimeout()
    {
        return MessageCode == "1907";
    }
}