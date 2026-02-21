using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Queue.Message;
using RabbitMqCommunicator;

namespace MyIndustry.Identity.Domain.Service;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICustomMessagePublisher _customMessagePublisher;
    private readonly IConfiguration _configuration;
    
    private const string EmailConfirmationPurpose = "EmailConfirmation";

    public UserService(
        UserManager<ApplicationUser> userManager, 
        ICustomMessagePublisher customMessagePublisher,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _customMessagePublisher = customMessagePublisher;
        _configuration = configuration;
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
            throw new Exception("User not found");
        }

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
            await PublishUserCreatedMessages(user, cancellationToken);
            return true;
        }
        else
        {
            throw new Exception("Error confirming your email.");
        }
    }
    
    public async Task<bool> ConfirmEmailByCode(string email, string code, CancellationToken cancellationToken)
    {
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
            throw new BusinessRuleException("Doğrulama kodu geçersiz veya süresi dolmuş.");
        }

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

    // ============ Phone Verification ============
    
    private const string PhoneChangePurpose = "PhoneChange";
    private const string EmailChangePurpose = "EmailChange";
    
    // Pending phone/email changes stored temporarily (in production, use Redis)
    private static readonly Dictionary<string, string> PendingPhoneChanges = new();
    private static readonly Dictionary<string, string> PendingEmailChanges = new();

    public async Task<bool> SendPhoneVerificationCode(string userId, string newPhoneNumber, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        // Generate 6-digit code using ASP.NET Identity's token provider
        var code = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, PhoneChangePurpose);
        
        // Store pending phone change
        PendingPhoneChanges[userId] = newPhoneNumber;
        
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
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        if (!PendingPhoneChanges.TryGetValue(userId, out var newPhoneNumber))
            throw new BusinessRuleException("Bekleyen telefon değişikliği bulunamadı.");

        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, PhoneChangePurpose, code);
        if (!isValid)
            throw new BusinessRuleException("Doğrulama kodu geçersiz veya süresi dolmuş.");

        // Update phone number
        user.PhoneNumber = newPhoneNumber;
        user.PhoneNumberConfirmed = true;
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            PendingPhoneChanges.Remove(userId);
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
        PendingEmailChanges[userId] = newEmail;
        
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
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new BusinessRuleException("Kullanıcı bulunamadı.");

        if (!PendingEmailChanges.TryGetValue(userId, out var newEmail))
            throw new BusinessRuleException("Bekleyen email değişikliği bulunamadı.");

        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, EmailChangePurpose, code);
        if (!isValid)
            throw new BusinessRuleException("Doğrulama kodu geçersiz veya süresi dolmuş.");

        // Update email
        user.Email = newEmail;
        user.UserName = newEmail; // Username is email
        user.NormalizedEmail = newEmail.ToUpperInvariant();
        user.NormalizedUserName = newEmail.ToUpperInvariant();
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            PendingEmailChanges.Remove(userId);
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
