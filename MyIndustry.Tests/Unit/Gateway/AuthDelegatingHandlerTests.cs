using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using MyIndustry.Container.Logging;
using MyIndustry.Gateway.Handlers;
using MyIndustry.Tests.Helpers;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MyIndustry.Tests.Unit.Gateway;

public class AuthDelegatingHandlerTests
{
    private const string SigningKey = "DevelopmentSigningKey-Min32CharsLong-ChangeInProduction";

    static AuthDelegatingHandlerTests()
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    }

    private readonly TokenValidationParameters _validationParameters;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public AuthDelegatingHandlerTests()
    {
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = "myindustry",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public async Task SendAsync_Should_Return_Unauthorized_When_No_Authorization_Header()
    {
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        using var client = CreateClient(HttpStatusCode.OK);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_Should_Forward_Request_With_User_Headers_When_Token_Valid()
    {
        var userId = Guid.NewGuid();
        var token = TestAuthHelper.CreateJwtToken(userId, "user@example.com");
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {token}";
        context.Items[CorrelationIdConstants.ItemKey] = "gateway-corr-id";
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        HttpRequestMessage? forwardedRequest = null;
        using var client = CreateClient(HttpStatusCode.OK, req => forwardedRequest = req);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        forwardedRequest.Should().NotBeNull();
        forwardedRequest!.Headers.GetValues("UserId").First().Should().Be(userId.ToString());
        forwardedRequest.Headers.GetValues("Email").First().Should().Be("user@example.com");
        forwardedRequest.Headers.GetValues(CorrelationIdConstants.HeaderName).First().Should().Be("gateway-corr-id");
    }

    [Fact]
    public async Task SendAsync_Should_Return_Unauthorized_When_Token_Expired()
    {
        var expiredToken = CreateExpiredToken();
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {expiredToken}";
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        using var client = CreateClient(HttpStatusCode.OK);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.ReasonPhrase.Should().Be("Token expired");
    }

    [Fact]
    public async Task SendAsync_Should_Return_Unauthorized_When_Token_Invalid()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer invalid.token.value";
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        using var client = CreateClient(HttpStatusCode.OK);

        var response = await client.GetAsync("http://localhost/api/test");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private HttpClient CreateClient(HttpStatusCode statusCode, Action<HttpRequestMessage>? onSend = null)
    {
        var innerHandlerMock = new Mock<HttpMessageHandler>();
        innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => onSend?.Invoke(req))
            .ReturnsAsync(new HttpResponseMessage(statusCode));

        var handler = new AuthDelegatingHandler(_httpContextAccessorMock.Object, _validationParameters)
        {
            InnerHandler = innerHandlerMock.Object
        };

        return new HttpClient(handler);
    }

    private static string CreateExpiredToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            audience: "myindustry",
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim("uid", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, "expired@example.com")
            },
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
