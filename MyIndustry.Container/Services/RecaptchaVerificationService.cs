using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace MyIndustry.Container.Services;

public interface IRecaptchaVerificationService
{
    Task<bool> VerifyAsync(string? token, CancellationToken cancellationToken = default);
}

public class RecaptchaVerificationService : IRecaptchaVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly string? _secretKey;

    public RecaptchaVerificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Recaptcha:SecretKey"];
    }

    public async Task<bool> VerifyAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            return true;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        using var response = await _httpClient.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={Uri.EscapeDataString(_secretKey)}&response={Uri.EscapeDataString(token)}",
            null,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>(cancellationToken: cancellationToken);
        return result?.Success == true;
    }

    private sealed class RecaptchaResponse
    {
        public bool Success { get; set; }
    }
}
