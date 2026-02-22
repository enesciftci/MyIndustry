using System.Text;
using System.Text.Json;

namespace MyIndustry.Identity.Api.Services;

/// <summary>
/// Kayıt sonrası kullanıcının kabul ettiği sözleşmeleri Main API'ye iletir.
/// </summary>
public interface IMainApiLegalDocumentAcceptanceClient
{
    Task SaveUserLegalDocumentAcceptancesAsync(Guid userId, List<Guid> legalDocumentIds, CancellationToken cancellationToken = default);
}

public class MainApiLegalDocumentAcceptanceClient : IMainApiLegalDocumentAcceptanceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public MainApiLegalDocumentAcceptanceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SaveUserLegalDocumentAcceptancesAsync(Guid userId, List<Guid> legalDocumentIds, CancellationToken cancellationToken = default)
    {
        if (legalDocumentIds == null || !legalDocumentIds.Any())
            return;

        var baseUrl = _configuration["MainApiUrl"]?.TrimEnd('/');
        var apiKey = _configuration["InternalApiKey"];
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
            return;

        var url = $"{baseUrl}/api/v1/Internal/user-legal-document-acceptances";
        _httpClient.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
        _httpClient.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

        try
        {
            var body = new { UserId = userId, LegalDocumentIds = legalDocumentIds };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                // Log but don't fail registration
                Console.WriteLine($"Main API legal document acceptances save failed: {response.StatusCode}");
            }
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
        }
    }
}
