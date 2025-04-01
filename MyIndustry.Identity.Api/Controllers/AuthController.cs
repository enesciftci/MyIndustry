using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
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

    public AuthController(IAuthService authService, IRedisCommunicator redisCommunicator, IUserService userService)
    {
        _authService = authService;
        _redisCommunicator = redisCommunicator;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var redisAuthModel =
            await _redisCommunicator.GetCacheValueAsync<AuthenticationModel>($"auth:{loginModel.Email}");

        if (redisAuthModel is not null)
            return Ok(redisAuthModel);

        var response = await _authService.GetTokenAsync(loginModel.Email, loginModel.Password);
        if (!response.IsAuthenticated)
            return Unauthorized(response);


        await _redisCommunicator.SetCacheValueAsync($"auth:{loginModel.Email}", response, TimeSpan.FromHours(24));


        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogOut([FromBody] LogOutModel logOutModel)
    {
        var userId = User.Claims.FirstOrDefault(p => p.Type == "uid")?.Value;

        var isTokenDeleted = await _authService.RemoveTokenAsync(userId);

        if (isTokenDeleted)
            return Ok();

        return new BadRequestResult();
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
        await _userService.VerifyTwoFactorCode(twoFactorVerificationModel, cancellationToken);
        return Ok();
    }
}