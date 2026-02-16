using System.Web;
using Microsoft.AspNetCore.Identity;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Queue.Message;
using RabbitMqCommunicator;

namespace MyIndustry.Identity.Domain.Service;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICustomMessagePublisher _customMessagePublisher;

    public UserService(UserManager<ApplicationUser> userManager, ICustomMessagePublisher customMessagePublisher)
    {
        _userManager = userManager;
        _customMessagePublisher = customMessagePublisher;
    }

    public async Task CreateUser(RegisterModel register, CancellationToken cancellationToken)
    {
        if (!string.Equals(register.Password, register.ConfirmPassword))
            throw new Exception("Passwords do not match");
        
        var user = new ApplicationUser()
        {
            Email = register.Email,
            UserName = register.Email,
            Type = register.UserType,
            FirstName = register.FirstName,
            LastName = register.LastName
        };
        
        var result = await _userManager.CreateAsync(user, register.Password);

        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            var confirmationLink = $"http://localhost:3000/email-verification?userId={user.Id}&token={encodedToken}";
            
            await _customMessagePublisher.Publish(new SendConfirmationEmailMessage() { 
                Email = user.Email,
                Subject = "Confirm your email",
                Body = confirmationLink }, cancellationToken);
        }

        // burda kullanıcıya email gönder kodu tabloya yaz 
        //
        // await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
        //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        //
    }

    public async Task VerifyTwoFactorCode(TwoFactorVerificationModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Kodun geçerliliğini doğrula
        var result = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, model.VerificationCode);

        if (result == false)
            throw new Exception("Code not wrong");
    }
    
    public async Task<bool> ConfirmEmail(string userId, string token, CancellationToken cancellationToken)
    {
        if (userId == null || token == null)
        {
            throw new Exception("User id or token is invalid");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new Exception($"Unable to load user with ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            await _customMessagePublisher.Publish(new CreateSellerMessage()
            {
                UserId = Guid.Parse(user.Id),
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            }, cancellationToken);
            
            await _customMessagePublisher.Publish(new CreatePurchaserMessage()
            {
                UserId = Guid.Parse(user.Id),
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            }, cancellationToken);
            return true;
        }
        else
        {
            throw new Exception("Error confirming your email.");
        }
    }

    public async Task SendConfirmationEmailMessage(ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);

        var confirmationLink = $"http://localhost:3000/email-verification?userId={user.Id}&token={encodedToken}";

        await _customMessagePublisher.Publish(new SendConfirmationEmailMessage()
        {
            Email = user.Email,
            Subject = "Confirm your email",
            Body = confirmationLink
        }, cancellationToken);
    }

    public async Task<ApplicationUser> GetUserByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> ForgotPassword(string email, string clientUrl, CancellationToken cancellationToken)
    {
        var user = await GetUserByEmail(email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return true; // Güvenlik için her zaman OK döneriz

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = HttpUtility.UrlEncode(token);
        var callbackUrl = $"{clientUrl}/reset-password?userId={user.Id}&token={encodedToken}";

        await _customMessagePublisher.Publish(new SendForgotPasswordEmailMessage() { 
            Email = user.Email,
            Subject = "Reset your password",
            Body = callbackUrl }, cancellationToken);
        return true;
    }
    
    public async Task<bool> ResetPassword(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new BusinessRuleException(result.Errors.First().Description);

        return true;
    }

    public UserDto GetUserById(string id)
    {
        var user = _userManager.Users.Select(p=>new UserDto()
        {
            Id = p.Id,
            Email = p.Email,
            UserName = p.UserName,
            FirstName = p.FirstName,
            LastName = p.LastName,
            PhoneNumber = p.PhoneNumber,
            UserType = p.Type
        }).SingleOrDefault(u => u.Id == id);

        return user;
    }
}

public interface IUserService
{
    Task CreateUser(RegisterModel register, CancellationToken cancellationToken);
    Task VerifyTwoFactorCode(TwoFactorVerificationModel model);
    Task<bool> ConfirmEmail(string userId, string token, CancellationToken cancellationToken);
    Task SendConfirmationEmailMessage(ApplicationUser user, CancellationToken cancellationToken);
    Task<ApplicationUser> GetUserByEmail(string email);
    Task<bool> ForgotPassword (string email, string clientUrl, CancellationToken cancellationToken);
    Task<bool> ResetPassword (string userId, string token, string newPassword);
    UserDto GetUserById(string id);
}