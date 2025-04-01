using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
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
        var user = new ApplicationUser() {Email = register.Email, UserName = register.Email};
        var result = await _userManager.CreateAsync(user, register.Password);  // Bu işlem şifreyi hash'ler.

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            
            await _customMessagePublisher.Publish(new SendVerificationCodeMessage(){Code = code}, cancellationToken);
        }

        // burda kullanıcıya email gönder kodu tabloya yaz 
        //
        // await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
        //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        //
    }

    public async Task VerifyTwoFactorCode(TwoFactorVerificationModel model, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(model.Email);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Kodun geçerliliğini doğrula
        var result = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, model.VerificationCode);

        if(result == false)
            throw new Exception("Code not wrong");
    }
}

public interface IUserService
{
    Task CreateUser(RegisterModel register, CancellationToken cancellationToken);
    Task VerifyTwoFactorCode(TwoFactorVerificationModel model, CancellationToken cancellationToken);
}