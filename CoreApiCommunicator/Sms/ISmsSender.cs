namespace CoreApiCommunicator.Sms;

public interface ISmsSender
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}
