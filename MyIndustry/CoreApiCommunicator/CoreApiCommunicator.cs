using CoreApiCommunicator.Request;
using CoreApiCommunicator.Response;

namespace CoreApiCommunicator;

public class CoreApiCommunicator<TRequest,TResponse> : 
    BaseCommunicator<TRequest,TResponse> ,ICoreApiCommunicator<TRequest,TResponse>  where TRequest : RequestBase where TResponse : ResponseBase
{
    public CoreApiCommunicator(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    public async Task<TResponse> CreateSeller(TRequest request, CancellationToken cancellationToken)
    {
        return await PostAsync("core-api", "api/v1/sellers", request, cancellationToken);
    }

    public async Task<TResponse> CreatePurchaser(TRequest request, CancellationToken cancellationToken)
    {
        return await PostAsync("core-api", "api/v1/purchasers", request, cancellationToken);
    }

    public async Task<TResponse> IncreaseServiceViewCount(TRequest request, CancellationToken cancellationToken)
    {
        return await PostAsync("core-api", "api/v1/purchasers", request, cancellationToken);
    }
}

public interface ICoreApiCommunicator<TRequest,TResponse>
{
    Task<TResponse> CreateSeller(TRequest request, CancellationToken cancellationToken);
    Task<TResponse> CreatePurchaser(TRequest request, CancellationToken cancellationToken);
    Task<TResponse> IncreaseServiceViewCount(TRequest request, CancellationToken cancellationToken);
}