using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreApiCommunicator;

public abstract class BaseCommunicator<TRequest, TResponse>
{
    private readonly IHttpClientFactory _httpClientFactory;

    protected BaseCommunicator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<TResponse> GetAsync(string clientName,string resource, string query, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(clientName);

        var httpResponseMessage = await client.GetAsync(resource+query, cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

        var content =  await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
        return content != Stream.Null
            ? await JsonSerializer.DeserializeAsync<TResponse>(content, cancellationToken: cancellationToken)
            : default;
    }
    
    public async Task<TResponse> PostAsync(string clientName,string resource, TRequest request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(clientName);

        var httpResponseMessage = await client.PostAsJsonAsync(resource, request, cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

        var content =  await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
        return content != Stream.Null
            ? await JsonSerializer.DeserializeAsync<TResponse>(content, cancellationToken: cancellationToken)
            : default;
    }
}