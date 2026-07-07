using System.Net;

namespace MyIndustry.Tests.Smoke;

internal static class SmokeAssertions
{
    private static readonly HashSet<HttpStatusCode> ValidStatuses =
    [
        HttpStatusCode.OK,
        HttpStatusCode.Created,
        HttpStatusCode.NoContent,
        HttpStatusCode.BadRequest,
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden,
        HttpStatusCode.NotFound,
        HttpStatusCode.TooManyRequests
    ];

    public static void AssertValidSmokeResponse(HttpResponseMessage response)
    {
        Assert.True(
            ValidStatuses.Contains(response.StatusCode),
            $"Expected smoke-acceptable status but got {(int)response.StatusCode} {response.StatusCode} for {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}");
    }
}
