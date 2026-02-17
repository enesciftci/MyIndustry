using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Identity.Domain.Repository;
using MyIndustry.Queue.Message;
using RabbitMqCommunicator;

namespace MyIndustry.Identity.Domain.Service;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICustomMessagePublisher _customMessagePublisher;
    private readonly IConfiguration _configuration;
    private readonly IEmailVerificationCodeRepository _verificationCodeRepository;

    public UserService(
        UserManager<ApplicationUser> userManager, 
        ICustomMessagePublisher customMessagePublisher,
        IConfiguration configuration,
        IEmailVerificationCodeRepository verificationCodeRepository)
    {
        _userManager = userManager;
        _customMessagePublisher = customMessagePublisher;
        _configuration = configuration;
        _verificationCodeRepository = verificationCodeRepository;
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
            await SendEmailVerificationAsync(user, cancellationToken);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }
    }

    private async Task SendEmailVerificationAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        // Generate token for email confirmation (ASP.NET Identity token)
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        
        // Generate 6-digit verification code
        var verificationCode = EmailTemplateHelper.GenerateVerificationCode();
        
        // Get frontend URL from configuration
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var confirmationLink = $"{frontendUrl}/email-verification?userId={user.Id}&token={encodedToken}";
        
        // Save verification code to database
        var emailVerificationCode = new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = user.Email,
            Code = verificationCode,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };
        
        await _verificationCodeRepository.AddAsync(emailVerificationCode, cancellationToken);
        
        // Generate HTML email template
        var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : null;
        var emailBody = EmailTemplateHelper.GetEmailConfirmationTemplate(userName, verificationCode, confirmationLink);
        
        // Publish message to queue
        await _customMessagePublisher.Publish(new SendConfirmationEmailMessage
        {
            Email = user.Email,
            Subject = "MyIndustry - Email Doğrulama",
            Body = emailBody
        }, cancellationToken);
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
            // Mark verification code as used
            var verificationCode = await _verificationCodeRepository.GetLatestByUserIdAndTokenAsync(userId, token);
            
            if (verificationCode != null)
            {
                verificationCode.IsUsed = true;
                await _verificationCodeRepository.UpdateAsync(verificationCode, cancellationToken);
            }
            
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
    
    public async Task<bool> ConfirmEmailByCode(string email, string code, CancellationToken cancellationToken)
    {
        var verificationCode = await _verificationCodeRepository.GetLatestByEmailAndCodeAsync(email, code);

        if (verificationCode == null)
        {
            throw new BusinessRuleException("Doğrulama kodu bulunamadı.");
        }

        if (verificationCode.IsExpired)
        {
            throw new BusinessRuleException("Doğrulama kodunun süresi dolmuş. Lütfen yeni kod talep edin.");
        }

        var user = await _userManager.FindByIdAsync(verificationCode.UserId);
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, verificationCode.Token);

        if (result.Succeeded)
        {
            verificationCode.IsUsed = true;
            await _verificationCodeRepository.UpdateAsync(verificationCode, cancellationToken);

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

        throw new Exception("Email doğrulama başarısız oldu.");
    }

    public async Task SendConfirmationEmailMessage(ApplicationUser user, CancellationToken cancellationToken)
    {
        await SendEmailVerificationAsync(user, cancellationToken);
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

        var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : null;
        var emailBody = EmailTemplateHelper.GetPasswordResetTemplate(userName, callbackUrl);

        await _customMessagePublisher.Publish(new SendForgotPasswordEmailMessage
        {
            Email = user.Email,
            Subject = "MyIndustry - Şifre Sıfırlama",
            Body = emailBody
        }, cancellationToken);
        
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
    
    public async Task ResendVerificationCode(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new BusinessRuleException("Kullanıcı bulunamadı.");
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            throw new BusinessRuleException("Email adresi zaten doğrulanmış.");
        }

        await SendEmailVerificationAsync(user, cancellationToken);
    }
}

public interface IUserService
{
    Task CreateUser(RegisterModel register, CancellationToken cancellationToken);
    Task VerifyTwoFactorCode(TwoFactorVerificationModel model);
    Task<bool> ConfirmEmail(string userId, string token, CancellationToken cancellationToken);
    Task<bool> ConfirmEmailByCode(string email, string code, CancellationToken cancellationToken);
    Task SendConfirmationEmailMessage(ApplicationUser user, CancellationToken cancellationToken);
    Task<ApplicationUser> GetUserByEmail(string email);
    Task<bool> ForgotPassword (string email, string clientUrl, CancellationToken cancellationToken);
    Task<bool> ResetPassword (string userId, string token, string newPassword);
    UserDto GetUserById(string id);
    Task ResendVerificationCode(string email, CancellationToken cancellationToken);
}
