using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreApiCommunicator.Sms;

/// <summary>
/// SMS Sender - Placeholder implementation
/// TODO: Integrate with actual SMS provider (Netgsm, Iletimerkezi, Twilio, etc.)
/// </summary>
public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;
    private readonly string _provider;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _sender;
    private readonly bool _isEnabled;

    public SmsSender(IConfiguration configuration, ILogger<SmsSender> logger)
    {
        _logger = logger;
        _provider = configuration["Sms:Provider"] ?? "placeholder";
        _apiKey = configuration["Sms:ApiKey"] ?? "";
        _apiSecret = configuration["Sms:ApiSecret"] ?? "";
        _sender = configuration["Sms:Sender"] ?? "MYINDUSTRY";
        _isEnabled = bool.Parse(configuration["Sms:Enabled"] ?? "false");
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        // Normalize phone number (remove spaces, add country code if missing)
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        
        _logger.LogInformation("SMS Request - To: {Phone}, Message: {Message}", normalizedPhone, message);

        if (!_isEnabled)
        {
            _logger.LogWarning("SMS sending is disabled. Would have sent to {Phone}: {Message}", normalizedPhone, message);
            // Development modda console'a yazdır
            Console.WriteLine($"[SMS PLACEHOLDER] To: {normalizedPhone}, Message: {message}");
            return true; // Development'ta başarılı say
        }

        try
        {
            // TODO: Implement actual SMS provider integration
            // Example providers:
            // - Netgsm: https://www.netgsm.com.tr
            // - İletimerkezi: https://www.iletimerkezi.com
            // - Twilio: https://www.twilio.com
            
            switch (_provider.ToLower())
            {
                case "netgsm":
                    return await SendViaNetgsm(normalizedPhone, message);
                case "iletimerkezi":
                    return await SendViaIletimerkezi(normalizedPhone, message);
                case "twilio":
                    return await SendViaTwilio(normalizedPhone, message);
                default:
                    _logger.LogWarning("Unknown SMS provider: {Provider}. Using placeholder.", _provider);
                    return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", normalizedPhone);
            return false;
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove spaces, dashes, parentheses
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        // Add Turkey country code if missing
        if (cleaned.StartsWith("0"))
            cleaned = "+90" + cleaned.Substring(1);
        else if (!cleaned.StartsWith("+"))
            cleaned = "+90" + cleaned;
            
        return cleaned;
    }

    private async Task<bool> SendViaNetgsm(string phone, string message)
    {
        // TODO: Implement Netgsm API
        // POST https://api.netgsm.com.tr/sms/send/get
        _logger.LogInformation("Netgsm SMS would be sent to {Phone}", phone);
        await Task.CompletedTask;
        return true;
    }

    private async Task<bool> SendViaIletimerkezi(string phone, string message)
    {
        // TODO: Implement İletimerkezi API
        // POST https://api.iletimerkezi.com/v1/send-sms
        _logger.LogInformation("Iletimerkezi SMS would be sent to {Phone}", phone);
        await Task.CompletedTask;
        return true;
    }

    private async Task<bool> SendViaTwilio(string phone, string message)
    {
        // TODO: Implement Twilio API
        // Uses Twilio SDK
        _logger.LogInformation("Twilio SMS would be sent to {Phone}", phone);
        await Task.CompletedTask;
        return true;
    }
}
