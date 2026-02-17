using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.Identity.Api.Requests;
using MyIndustry.Identity.Domain.Service;
using RedisCommunicator;

namespace MyIndustry.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRedisCommunicator _redisCommunicator;
    private readonly IUserService _userService;

    public AuthController(
        IAuthService authService, 
        IRedisCommunicator redisCommunicator, 
        IUserService userService)
    {
        _authService = authService;
        _redisCommunicator = redisCommunicator;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel, CancellationToken cancellationToken)
    {
        var redisAuthModel =
            await _redisCommunicator.GetCacheValueAsync<AuthenticationModel>($"auth:{loginModel.Email}");

        if (redisAuthModel is not null)
            return Ok(redisAuthModel);

        var user = await _userService.GetUserByEmail(loginModel.Email);
        if (user is null)
            return Unauthorized(new { Message = "Geçersiz kullanıcı adı veya şifre." });
        
        if (!user.EmailConfirmed)
        {
            await _userService.SendConfirmationEmailMessage(user, cancellationToken);
            
            return BadRequest(new
            {
                Message = "E-posta adresiniz henüz doğrulanmamış.",
                RequiresEmailConfirmation = true
            });
        }
        
        var response = await _authService.GetTokenAsync(loginModel.Email, loginModel.Password);
        if (!response.IsAuthenticated)
            return Unauthorized(response);

        response.User = new UserDto()
        {
            Id = user.Id,
            Email = user.Email
        };
        
        await _redisCommunicator.SetCacheValueAsync($"auth:{loginModel.Email}", response, TimeSpan.FromHours(2));
        
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogOut()
    {
        var userId = User.Claims.FirstOrDefault(p => p.Type == "uid")?.Value;

        await _authService.RemoveTokenAsync(userId);

        return Ok();
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel registerModel, CancellationToken cancellationToken)
    {
         await _userService.CreateUser(registerModel, cancellationToken);

         return Ok();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] TwoFactorVerificationModel twoFactorVerificationModel, CancellationToken cancellationToken)
    {
        await _userService.VerifyTwoFactorCode(twoFactorVerificationModel);
        return Ok();
    }
    
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] EmailConfirmationDto emailConfirmationDto, CancellationToken cancellationToken)
    {
        var result = await _userService.ConfirmEmail(emailConfirmationDto.UserId, emailConfirmationDto.Token, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost("confirm-email-by-code")]
    public async Task<IActionResult> ConfirmEmailByCode([FromBody] EmailConfirmationByCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.ConfirmEmailByCode(request.Email, request.Code, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost("resend-verification-code")]
    public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeRequest request, CancellationToken cancellationToken)
    {
        await _userService.ResendVerificationCode(request.Email, cancellationToken);
        return Ok(new { Message = "Doğrulama kodu tekrar gönderildi." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] PasswordForgotRequest forgotPasswordRequest, CancellationToken cancellationToken)
    {
        var result = await _userService.ForgotPassword(forgotPasswordRequest.Email,forgotPasswordRequest.ClientUrl, cancellationToken);
    
        return Ok(result);
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
    {
        var result = await _userService.ResetPassword(request.UserId, request.Token, request.NewPassword);
    
        return Ok(result);
    }

    [HttpGet]
    public IActionResult Get()
    {
        var userId = User.Claims.FirstOrDefault(p => p.Type == "uid")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = _userService.GetUserById(userId);

        return Ok(user);
    }
}