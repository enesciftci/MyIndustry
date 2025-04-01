using CoreApiCommunicator.Request;
using CoreApiCommunicator.Response;

namespace CoreApiCommunicator;

public class CoreApiCommunicator<TRequest,TResponse> : BaseCommunicator<TRequest,TResponse>  where TRequest : RequestBase where TResponse : ResponseBase
{
    public CoreApiCommunicator(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    public async Task<TResponse> CreateSeller(TRequest request, CancellationToken cancellationToken)
    {
        return await PostAsync("core-api", "api/v1/sellers", request, cancellationToken);
    }
}

public interface ICoreApiCommunicator<TRequest,TResponse>
{
    Task<TResponse> CreateSeller(TRequest request, CancellationToken cancellationToken);
}