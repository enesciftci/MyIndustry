using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Identity.Domain.Service;
using RedisCommunicator;

namespace MyIndustry.Tests.Unit.Identity;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IRedisCommunicator> _redisMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _redisMock = new Mock<IRedisCommunicator>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "DevelopmentSigningKey-Min32CharsLong-ChangeInProduction"
            })
            .Build();
        _authService = new AuthService(_userManagerMock.Object, _redisMock.Object, _configuration);
    }

    [Fact]
    public async Task GetTokenAsync_Should_Return_Unauthenticated_When_User_Not_Found()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("missing@example.com")).ReturnsAsync((ApplicationUser?)null);

        var result = await _authService.GetTokenAsync("missing@example.com", "password");

        result.IsAuthenticated.Should().BeFalse();
        result.Message.Should().Be("Geçersiz kullanıcı adı veya şifre.");
    }

    [Fact]
    public async Task GetTokenAsync_Should_Return_Unauthenticated_When_Password_Invalid()
    {
        var user = CreateUser("user@example.com");
        _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "wrong-password")).ReturnsAsync(false);

        var result = await _authService.GetTokenAsync("user@example.com", "wrong-password");

        result.IsAuthenticated.Should().BeFalse();
        result.Message.Should().Be("Geçersiz kullanıcı adı veya şifre.");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Unauthenticated_When_Token_Expired()
    {
        _redisMock.Setup(r => r.GetCacheValueAsync<RefreshTokenData>("refresh:expired-token"))
            .ReturnsAsync(new RefreshTokenData
            {
                UserId = Guid.NewGuid().ToString(),
                Email = "user@example.com",
                Expiry = DateTime.UtcNow.AddHours(-1)
            });

        var result = await _authService.RefreshTokenAsync("expired-token");

        result.IsAuthenticated.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Unauthenticated_When_Token_Not_Found()
    {
        _redisMock.Setup(r => r.GetCacheValueAsync<RefreshTokenData>("refresh:missing-token"))
            .ReturnsAsync((RefreshTokenData?)null!);

        var result = await _authService.RefreshTokenAsync("missing-token");

        result.IsAuthenticated.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RemoveTokenAsync_Should_Return_False_When_User_Not_Found()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var result = await _authService.RemoveTokenAsync(Guid.NewGuid().ToString());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveTokenAsync_Should_Delete_Redis_Cache_When_User_Exists()
    {
        var user = CreateUser("user@example.com");
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _redisMock.Setup(r => r.DeleteValue($"auth:{user.Email}")).Returns(true);

        var result = await _authService.RemoveTokenAsync(user.Id);

        result.Should().BeTrue();
        _redisMock.Verify(r => r.DeleteValue($"auth:{user.Email}"), Times.Once);
    }

    [Fact]
    public async Task AddTokenToBlacklistAsync_Should_Store_Token_In_Redis()
    {
        var jti = Guid.NewGuid().ToString();
        var exp = DateTime.UtcNow.AddHours(1);

        await _authService.AddTokenToBlacklistAsync(jti, exp);

        _redisMock.Verify(r => r.SetCacheValueAsync("jwt_blacklist:" + jti, "1", It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task AddTokenToBlacklistAsync_Should_Skip_When_Jti_Is_Empty()
    {
        await _authService.AddTokenToBlacklistAsync("", DateTime.UtcNow.AddHours(1));

        _redisMock.Verify(r => r.SetCacheValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task IsTokenBlacklistedAsync_Should_Return_True_When_Token_In_Blacklist()
    {
        var jti = Guid.NewGuid().ToString();
        _redisMock.Setup(r => r.GetCacheValueAsync<string>("jwt_blacklist:" + jti)).ReturnsAsync("1");

        var result = await _authService.IsTokenBlacklistedAsync(jti);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTokenBlacklistedAsync_Should_Return_False_When_Token_Not_In_Blacklist()
    {
        var jti = Guid.NewGuid().ToString();
        _redisMock.Setup(r => r.GetCacheValueAsync<string>("jwt_blacklist:" + jti)).ReturnsAsync((string?)null!);

        var result = await _authService.IsTokenBlacklistedAsync(jti);

        result.Should().BeFalse();
    }

    private static ApplicationUser CreateUser(string email)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            FirstName = "Test",
            LastName = "User"
        };
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
