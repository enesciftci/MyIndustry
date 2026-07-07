using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.Api.Controllers;
using MyIndustry.ApplicationService.Handler;

namespace MyIndustry.Tests.Smoke;

public class BaseControllerTests
{
    private sealed class TestableBaseController : BaseController
    {
        public IActionResult InvokeCreateResponse(ResponseBase response) => CreateResponse(response);
        public Guid InvokeGetUserId() => GetUserId();
        public string InvokeGetUserEmail() => GetUserEmail();
        public string InvokeGetUserName() => GetUserName();
        public bool InvokeIsAdmin() => IsAdmin();
    }

    [Fact]
    public void CreateResponse_WithSuccess_ReturnsOk()
    {
        var controller = new TestableBaseController();
        var response = new ResponseBase().ReturnOk();
        var result = controller.InvokeCreateResponse(response);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void CreateResponse_WithFailure_ReturnsBadRequest()
    {
        var controller = new TestableBaseController();
        var response = new ResponseBase().ReturnBad();
        var result = controller.InvokeCreateResponse(response);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetUserId_WithValidClaim_ReturnsGuid()
    {
        var userId = Guid.NewGuid();
        var controller = CreateControllerWithClaims(new Claim("uid", userId.ToString()));
        Assert.Equal(userId, controller.InvokeGetUserId());
    }

    [Fact]
    public void GetUserId_WithoutClaim_ThrowsUnauthorizedAccessException()
    {
        var controller = new TestableBaseController
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        Assert.Throws<UnauthorizedAccessException>(() => controller.InvokeGetUserId());
    }

    [Fact]
    public void GetUserEmail_ReturnsEmailClaim()
    {
        var controller = CreateControllerWithClaims(new Claim("email", "test@example.com"));
        Assert.Equal("test@example.com", controller.InvokeGetUserEmail());
    }

    [Fact]
    public void GetUserName_ReturnsFullName()
    {
        var controller = CreateControllerWithClaims(
            new Claim("given_name", "John"),
            new Claim("family_name", "Doe"));
        Assert.Equal("John Doe", controller.InvokeGetUserName());
    }

    [Theory]
    [InlineData("99", true)]
    [InlineData("0", false)]
    [InlineData("2", false)]
    public void IsAdmin_ReturnsExpected(string type, bool expected)
    {
        var controller = CreateControllerWithClaims(new Claim("type", type));
        Assert.Equal(expected, controller.InvokeIsAdmin());
    }

    private static TestableBaseController CreateControllerWithClaims(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        return new TestableBaseController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }
}
