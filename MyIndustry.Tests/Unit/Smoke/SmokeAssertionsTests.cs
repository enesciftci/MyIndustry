using System.Net;
using FluentAssertions;
using MyIndustry.Tests.Smoke;

namespace MyIndustry.Tests.Unit.Smoke;

public class SmokeAssertionsTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public void AssertValidSmokeResponse_Accepts_NonServerError_StatusCodes(HttpStatusCode statusCode)
    {
        using var response = new HttpResponseMessage(statusCode);

        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public void AssertValidSmokeResponse_Rejects_InternalServerError()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var act = () => SmokeAssertions.AssertValidSmokeResponse(response);

        act.Should().Throw<Exception>();
    }
}
