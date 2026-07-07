using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Identity.Domain.Aggregate.ValueObjects;
using MyIndustry.Identity.Domain.Service;
using MyIndustry.Queue.Message;
using RabbitMqCommunicator;
using RedisCommunicator;

namespace MyIndustry.Tests.Unit.Identity;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ICustomMessagePublisher> _publisherMock;
    private readonly Mock<IRedisCommunicator> _redisMock;
    private readonly IConfiguration _configuration;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _publisherMock = new Mock<ICustomMessagePublisher>();
        _redisMock = new Mock<IRedisCommunicator>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FrontendUrl"] = "http://localhost:3000",
                ["PasswordReset:AllowedBaseUrl"] = "http://localhost:3000"
            })
            .Build();
        _userService = new UserService(_userManagerMock.Object, _publisherMock.Object, _configuration, _redisMock.Object);
    }

    [Fact]
    public async Task CreateUser_Should_Throw_When_Passwords_Do_Not_Match()
    {
        var register = new RegisterModel
        {
            Email = "user@example.com",
            Password = "Password1!",
            ConfirmPassword = "Password2!",
            FirstName = "Test",
            LastName = "User",
            UserType = UserType.User
        };

        var act = () => _userService.CreateUser(register, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Passwords do not match");
    }

    [Fact]
    public async Task CreateUser_Should_Throw_When_UserType_Is_Admin()
    {
        var register = new RegisterModel
        {
            Email = "attacker@example.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            FirstName = "Bad",
            LastName = "Actor",
            UserType = UserType.Admin
        };

        var act = () => _userService.CreateUser(register, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Geçersiz kullanıcı tipi*");
    }

    [Theory]
    [InlineData(UserType.User)]
    [InlineData(UserType.Seller)]
    public void ValidateRegistrationUserType_Should_Accept_Valid_Types(UserType userType)
    {
        UserService.ValidateRegistrationUserType(userType).Should().Be(userType);
    }

    [Theory]
    [InlineData(UserType.Admin)]
    [InlineData(UserType.Purchaser)]
    public void ValidateRegistrationUserType_Should_Reject_Invalid_Types(UserType userType)
    {
        var act = () => UserService.ValidateRegistrationUserType(userType);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public async Task CreateUser_Should_Publish_Confirmation_Email_On_Success()
    {
        var user = CreateUser("new@example.com");
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((u, _) => u.Id = user.Id);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirmation-token");

        var register = new RegisterModel
        {
            Email = "new@example.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            FirstName = "New",
            LastName = "User",
            UserType = UserType.User
        };

        var result = await _userService.CreateUser(register, CancellationToken.None);

        result.Should().NotBeNull();
        _publisherMock.Verify(p => p.Publish(It.Is<SendConfirmationEmailMessage>(m =>
            m.Email == "new@example.com" &&
            m.Subject.Contains("Doğrulayın") &&
            m.Body.Contains("Hesabımı Doğrula")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUser_Should_Throw_When_Identity_Create_Fails()
    {
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already taken" }));

        var register = new RegisterModel
        {
            Email = "exists@example.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            UserType = UserType.User
        };

        var act = () => _userService.CreateUser(register, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Email already taken*");
    }

    [Fact]
    public async Task ConfirmEmail_Should_Throw_When_UserId_Or_Token_Is_Null()
    {
        var act = () => _userService.ConfirmEmail(null!, "token", CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Kullanıcı kimliği veya token geçersiz.");
    }

    [Fact]
    public async Task ConfirmEmail_Should_Return_True_On_Success()
    {
        var user = CreateUser("confirmed@example.com");
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "valid-token")).ReturnsAsync(IdentityResult.Success);

        var result = await _userService.ConfirmEmail(user.Id, "valid-token", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_True_When_User_Not_Found()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("missing@example.com")).ReturnsAsync((ApplicationUser?)null);

        var result = await _userService.ForgotPassword("missing@example.com", "http://localhost:3000", CancellationToken.None);

        result.Should().BeTrue();
        _publisherMock.Verify(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_Should_Publish_Reset_Email_When_User_Exists()
    {
        var user = CreateUser("user@example.com");
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");

        var result = await _userService.ForgotPassword(user.Email, "http://localhost:3000", CancellationToken.None);

        result.Should().BeTrue();
        _publisherMock.Verify(p => p.Publish(It.Is<SendForgotPasswordEmailMessage>(m =>
            m.Email == user.Email &&
            m.Subject.Contains("Şifre Sıfırlama") &&
            m.Body.Contains("Şifremi Sıfırla")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_Should_Throw_When_User_Not_Found()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var act = () => _userService.ResetPassword(Guid.NewGuid().ToString(), "token", "NewPass1!");

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Kullanıcı bulunamadı.");
    }

    [Fact]
    public async Task GetUserByEmail_Should_Return_User()
    {
        var user = CreateUser("lookup@example.com");
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);

        var result = await _userService.GetUserByEmail(user.Email);

        result.Should().BeSameAs(user);
    }

    [Fact]
    public void GetUserById_Should_Return_UserDto()
    {
        var userId = Guid.NewGuid().ToString();
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = userId,
                Email = "lookup@example.com",
                FirstName = "Lookup",
                LastName = "User",
                Type = UserType.Seller
            }
        }.AsQueryable();

        _userManagerMock.Setup(m => m.Users).Returns(users);

        var result = _userService.GetUserById(userId);

        result.Should().NotBeNull();
        result!.Email.Should().Be("lookup@example.com");
        result.FirstName.Should().Be("Lookup");
        result.UserType.Should().Be(UserType.Seller);
    }

    [Fact]
    public async Task UpdateProfile_Should_Update_User_Names()
    {
        var user = CreateUser("profile@example.com");
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _userService.UpdateProfile(user.Id, "Updated", "Name", CancellationToken.None);

        result.Should().BeTrue();
        user.FirstName.Should().Be("Updated");
        user.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task SuspendUser_Should_Set_Suspended_Flags()
    {
        var user = CreateUser("suspend@example.com");
        user.Type = UserType.Seller;
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _userService.SuspendUser(user.Id, "Policy violation", CancellationToken.None);

        result.Should().BeTrue();
        user.IsSuspended.Should().BeTrue();
        user.SuspensionReason.Should().Be("Policy violation");
    }

    [Fact]
    public async Task SuspendUser_Should_Throw_For_Admin_User()
    {
        var user = CreateUser("admin@example.com");
        user.Type = UserType.Admin;
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var act = () => _userService.SuspendUser(user.Id, "reason", CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Admin kullanıcıları dondurulamaz.");
    }

    [Fact]
    public async Task UnsuspendUser_Should_Clear_Suspension()
    {
        var user = CreateUser("unsuspend@example.com");
        user.IsSuspended = true;
        user.SuspensionReason = "test";
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _userService.UnsuspendUser(user.Id, CancellationToken.None);

        result.Should().BeTrue();
        user.IsSuspended.Should().BeFalse();
        user.SuspensionReason.Should().BeNull();
    }

    [Fact]
    public async Task GetAllUsers_Should_Exclude_Admin_And_Apply_Search()
    {
        var seller = CreateUser("seller@example.com");
        seller.Type = UserType.Seller;
        seller.FirstName = "Seller";
        var admin = CreateUser("admin@admin.com");
        admin.Type = UserType.Admin;
        admin.FirstName = "Admin";
        var customer = CreateUser("customer@example.com");
        customer.Type = UserType.User;
        customer.FirstName = "Customer";
        var users = new List<ApplicationUser> { seller, admin, customer };
        _userManagerMock.Setup(m => m.Users).Returns(users.AsQueryable());

        var (result, totalCount) = await _userService.GetAllUsers(1, 10, "seller", null, CancellationToken.None);

        totalCount.Should().Be(1);
        result.Should().HaveCount(1);
        result[0].Email.Should().Be("seller@example.com");
    }

    private static ApplicationUser CreateUser(string email)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            FirstName = "Test",
            LastName = "User",
            Type = UserType.User
        };
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
