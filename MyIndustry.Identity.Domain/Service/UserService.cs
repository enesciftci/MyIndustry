using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Identity.Domain.Aggregate.ValueObjects;
using MyIndustry.Queue.Message;
using RabbitMqCommunicator;
using RedisCommunicator;

namespace MyIndustry.Identity.Domain.Service;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICustomMessagePublisher _customMessagePublisher;
    private readonly IConfiguration _configuration;
    private readonly IRedisCommunicator _redisCommunicator;
    
    private const string EmailConfirmationPurpose = "EmailConfirmation";
    private const int MaxVerificationAttempts = 5;
    private static readonly TimeSpan VerificationAttemptWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PendingChangeTtl = TimeSpan.FromMinutes(15);

    public UserService(
        UserManager<ApplicationUser> userManager, 
        ICustomMessagePublisher customMessagePublisher,
        IConfiguration configuration,
        IRedisCommunicator redisCommunicator)
    {
        _userManager = userManager;
        _customMessagePublisher = customMessagePublisher;
        _configuration = configuration;
        _redisCommunicator = redisCommunicator;
    }

    public async Task<Guid?> CreateUser(RegisterModel register, CancellationToken cancellationToken)
    {
        if (!string.Equals(register.Password, register.ConfirmPassword))
            throw new Exception("Passwords do not match");

        var userType = ValidateRegistrationUserType(register.UserType);
        
        var user = new ApplicationUser()
        {
            Email = register.Email,
            UserName = register.Email,
            Type = userType,
            FirstName = register.FirstName,
            LastName = register.LastName
        };
        
        var result = await _userManager.CreateAsync(user, register.Password);

        if (result.Succeeded)
        {
            await SendEmailVerificationAsync(user, cancellationToken);
            return Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }
    }

    private async Task SendEmailVerificationAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        // Generate token for email confirmation (link-based verification only)
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        
        // Get frontend URL from configuration
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var confirmationLink = $"{frontendUrl}/email-verification?userId={user.Id}&token={encodedToken}";
        
        // Generate HTML email template (link only - easier for users)
        var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : null;
        var emailBody = EmailTemplateHelper.GetEmailConfirmationTemplate(userName, confirmationLink);
        
        // Publish message to queue
        await _customMessagePublisher.Publish(new SendConfirmationEmailMessage
        {
            Email = user.Email,
            Subject = "MyIndustry - Hesabınızı Doğrulayın",
            Body = emailBody
        }, cancellationToken);
    }

    public async Task VerifyTwoFactorCode(TwoFactorVerificationModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            throw new BusinessRuleException("Kullanıcı bulunamadı.");
        }

        var result = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, model.VerificationCode);

        if (result == false)
            throw new BusinessRuleException("Doğrulama kodu geçersiz.");
    }
    
    public async Task<bool> ConfirmEmail(string userId, string token, CancellationToken cancellationToken)
    {
        if (userId == null || token == null)
        {
            throw new BusinessRuleException("Kullanıcı kimliği veya token geçersiz.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new BusinessRuleException($"Kullanıcı bulunamadı.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            await PublishUserCreatedMessages(user, cancellationToken);
            return true;
        }

        throw new BusinessRuleException("Email doğrulama başarısız.");
    }
    
    public async Task<bool> ConfirmEmailByCode(string email, string code, CancellationToken cancellationToken)
    {
        await EnsureVerificationAttemptsAllowedAsync($"verify_attempts:email_confirm:{email}");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new BusinessRuleException("Kullanıcı bulunamadı.");
        }

        if (user.EmailConfirmed)
        {
            throw new BusinessRuleException("Email adresi zaten doğrulanmış.");
        }

        // Verify the 6-digit code using Identity's token provider
        var isValid = await _userManager.VerifyUserTokenAsync(
            user, 
            TokenOptions.DefaultEmailProvider, 
            EmailConfirmationPurpose, 
            code);

        if (!isValid)
        {
            await IncrementVerificationAttemptsAsync($"verify_attempts:email_confirm:{email}");
            throw new BusinessRuleException("Doğrulama kodu geçersiz veya süresi dolmuş.");
        }

        _redisCommunicator.DeleteValue($"verify_attempts:email_confirm:{email}");

        // Mark email as confirmed
        user.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // Update security stamp to invalidate old codes
            await _userManager.UpdateSecurityStampAsync(user);
            
            await PublishUserCreatedMessages(user, cancellationToken);
            return true;
        }

        throw new Exception("Email doğrulama başarısız oldu.");
    }
    
    private Task PublishUserCreatedMessages(ApplicationUser user, CancellationToken cancellationToken)
    {
        // Purchaser entity kaldırıldı - User bilgileri artık sadece Identity'de tutuluyor
        // Seller profile, SellerSetup sayfasından oluşturuluyor
        return Task.CompletedTask;
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

        // Open redirect önlemi: ClientUrl yalnızca izin verilen base URL ile başlamalı (örn. FrontendUrl)
        var allowedBase = (_configuration["PasswordReset:AllowedBaseUrl"] ?? _configuration["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
        var baseUrl = !string.IsNullOrWhiteSpace(clientUrl) && clientUrl.Trim().StartsWith(allowedBase, StringComparison.OrdinalIgnoreCase)
            ? clientUrl.Trim().TrimEnd('/')
            : allowedBase;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = HttpUtility.UrlEncode(token);
        var callbackUrl = $"{baseUrl}/reset-password?userId={user.Id}&token={encodedToken}";

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

    // ============ Phone Verification ============
    
    private const string PhoneChangePurpose = "PhoneChange";
    private const string EmailChangePurpose = "EmailChange";
    
    // Pending phone/email changes stored in Redis with TTL
    
    public async Task<bool> SendPhoneVerificationCode(string userId, string newPhoneNumber, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        // Generate 6-digit code using ASP.NET Identity's token provider
        var code = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, PhoneChangePurpose);
        
        // Store pending phone change
        await _redisCommunicator.SetCacheValueAsync($"pending_phone:{userId}", newPhoneNumber, PendingChangeTtl);
        
        // Send SMS via RabbitMQ queue
        await _customMessagePublisher.Publish(new SendPhoneVerificationMessage 
        { 
            PhoneNumber = newPhoneNumber, 
            VerificationCode = code 
        }, cancellationToken);
        
        Console.WriteLine($"[PHONE VERIFICATION] Sent to queue - User: {userId}, Phone: {newPhoneNumber}");
        
        return true;
    }

    public async Task<bool> VerifyPhoneCode(string userId, string code, CancellationToken cancellationToken)
    {
        await EnsureVerificationAttemptsAllowedAsync($"verify_attempts:phone:{userId}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        var newPhoneNumber = await _redisCommunicator.GetCacheValueAsync<string>($"pending_phone:{userId}");
        if (string.IsNullOrWhiteSpace(newPhoneNumber))
            throw new BusinessRuleException("Bekleyen telefon değişikliği bulunamadı.");

        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, PhoneChangePurpose, code);
        if (!isValid)
        {
            await IncrementVerificationAttemptsAsync($"verify_attempts:phone:{userId}");
            throw new BusinessRuleException("Doğrulama kodu geçersiz veya süresi dolmuş.");
        }

        // Update phone number
        user.PhoneNumber = newPhoneNumber;
        user.PhoneNumberConfirmed = true;
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            _redisCommunicator.DeleteValue($"pending_phone:{userId}");
            _redisCommunicator.DeleteValue($"verify_attempts:phone:{userId}");
            await _userManager.UpdateSecurityStampAsync(user);
            return true;
        }

        throw new BusinessRuleException("Telefon numarası güncellenemedi.");
    }

    // ============ Email Change Verification ============

    public async Task<bool> SendEmailChangeVerificationCode(string userId, string newEmail, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        // Check if new email is already in use
        var existingUser = await _userManager.FindByEmailAsync(newEmail);
        if (existingUser != null && existingUser.Id != userId)
            throw new BusinessRuleException("Bu email adresi zaten kullanımda.");

        // Generate verification code
        var code = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, EmailChangePurpose);
        
        // Store pending email change
        await _redisCommunicator.SetCacheValueAsync($"pending_email:{userId}", newEmail, PendingChangeTtl);
        
        // Send verification email to NEW email address
        var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : null;
        var emailBody = EmailTemplateHelper.GetEmailChangeVerificationTemplate(userName, code);
        
        await _customMessagePublisher.Publish(new SendConfirmationEmailMessage
        {
            Email = newEmail,
            Subject = "MyIndustry - Email Değişikliği Doğrulama",
            Body = emailBody
        }, cancellationToken);

        return true;
    }

    public async Task<bool> VerifyEmailChangeCode(string userId, string code, CancellationToken cancellationToken)
    {
        await EnsureVerificationAttemptsAllowedAsync($"verify_attempts:email_change:{userId}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        var newEmail = await _redisCommunicator.GetCacheValueAsync<string>($"pending_email:{userId}");
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new BusinessRuleException("Bekleyen email değişikliği bulunamadı.");

        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, EmailChangePurpose, code);
        if (!isValid)
        {
            await IncrementVerificationAttemptsAsync($"verify_attempts:email_change:{userId}");
            throw new BusinessRuleException("Doğrulama kodu geçersiz veya süresi dolmuş.");
        }

        // Update email
        user.Email = newEmail;
        user.UserName = newEmail; // Username is email
        user.NormalizedEmail = newEmail.ToUpperInvariant();
        user.NormalizedUserName = newEmail.ToUpperInvariant();
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            _redisCommunicator.DeleteValue($"pending_email:{userId}");
            _redisCommunicator.DeleteValue($"verify_attempts:email_change:{userId}");
            await _userManager.UpdateSecurityStampAsync(user);
            return true;
        }

        throw new BusinessRuleException("Email adresi güncellenemedi.");
    }

    // ============ Profile Update ============

    public async Task<bool> UpdateProfile(string userId, string firstName, string lastName, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        user.FirstName = firstName;
        user.LastName = lastName;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BusinessRuleException("Profil güncellenemedi.");

        return true;
    }

    // ============ Admin Methods ============

    public async Task<(List<UserListDto> Users, int TotalCount)> GetAllUsers(int index, int size, string? search, int? userType, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        // Filter by user type if specified
        if (userType.HasValue)
        {
            query = query.Where(u => (int)u.Type == userType.Value);
        }

        // Search by email or name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(searchLower) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower)));
        }

        // Exclude admin users from list
        query = query.Where(u => (int)u.Type != 99);

        var totalCount = query.Count();

        var users = query
            .OrderByDescending(u => u.Id)
            .Skip((index - 1) * size)
            .Take(size)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                UserType = (int)u.Type,
                EmailConfirmed = u.EmailConfirmed,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                IsSuspended = u.IsSuspended,
                SuspensionReason = u.SuspensionReason,
                CreatedDate = DateTime.UtcNow // Identity doesn't track creation date by default
            })
            .ToList();

        return (users, totalCount);
    }

    public async Task<bool> SuspendUser(string userId, string? reason, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        // Admin kullanıcıları dondurulamaz
        if (user.Type == Aggregate.ValueObjects.UserType.Admin)
            throw new BusinessRuleException("Admin kullanıcıları dondurulamaz.");

        user.IsSuspended = true;
        user.SuspensionReason = reason;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BusinessRuleException("Kullanıcı dondurulamadı.");

        return true;
    }

    public async Task<bool> UnsuspendUser(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        user.IsSuspended = false;
        user.SuspensionReason = null;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BusinessRuleException("Kullanıcı aktifleştirilemedi.");

        return true;
    }

    public static UserType ValidateRegistrationUserType(UserType userType)
    {
        if (userType is UserType.User or UserType.Seller)
            return userType;

        throw new BusinessRuleException("Geçersiz kullanıcı tipi. Yalnızca alıcı veya satıcı olarak kayıt olabilirsiniz.");
    }

    private async Task EnsureVerificationAttemptsAllowedAsync(string key)
    {
        var attempts = await _redisCommunicator.GetCacheValueAsync<int?>(key) ?? 0;
        if (attempts >= MaxVerificationAttempts)
            throw new BusinessRuleException("Çok fazla deneme yapıldı. Lütfen daha sonra tekrar deneyin.");
    }

    private async Task IncrementVerificationAttemptsAsync(string key)
    {
        var attempts = await _redisCommunicator.GetCacheValueAsync<int?>(key) ?? 0;
        await _redisCommunicator.SetCacheValueAsync(key, attempts + 1, VerificationAttemptWindow);
    }
}
public interface IUserService
{
    Task<Guid?> CreateUser(RegisterModel register, CancellationToken cancellationToken);
    Task VerifyTwoFactorCode(TwoFactorVerificationModel model);
    Task<bool> ConfirmEmail(string userId, string token, CancellationToken cancellationToken);
    Task<bool> ConfirmEmailByCode(string email, string code, CancellationToken cancellationToken);
    Task SendConfirmationEmailMessage(ApplicationUser user, CancellationToken cancellationToken);
    Task<ApplicationUser> GetUserByEmail(string email);
    Task<bool> ForgotPassword (string email, string clientUrl, CancellationToken cancellationToken);
    Task<bool> ResetPassword (string userId, string token, string newPassword);
    UserDto GetUserById(string id);
    Task ResendVerificationCode(string email, CancellationToken cancellationToken);
    
    // Phone verification
    Task<bool> SendPhoneVerificationCode(string userId, string newPhoneNumber, CancellationToken cancellationToken);
    Task<bool> VerifyPhoneCode(string userId, string code, CancellationToken cancellationToken);
    
    // Email change verification
    Task<bool> SendEmailChangeVerificationCode(string userId, string newEmail, CancellationToken cancellationToken);
    Task<bool> VerifyEmailChangeCode(string userId, string code, CancellationToken cancellationToken);
    
    // Profile update
    Task<bool> UpdateProfile(string userId, string firstName, string lastName, CancellationToken cancellationToken);
    
    // Admin methods
    Task<(List<UserListDto> Users, int TotalCount)> GetAllUsers(int index, int size, string? search, int? userType, CancellationToken cancellationToken);
    Task<bool> SuspendUser(string userId, string? reason, CancellationToken cancellationToken);
    Task<bool> UnsuspendUser(string userId, CancellationToken cancellationToken);
}

public class UserListDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public int UserType { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTime CreatedDate { get; set; }
}
