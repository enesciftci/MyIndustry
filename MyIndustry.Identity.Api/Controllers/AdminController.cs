using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.Identity.Domain.Service;

namespace MyIndustry.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users with pagination and filtering
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int index = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? userType = null,
        CancellationToken cancellationToken = default)
    {
        var (users, totalCount) = await _userService.GetAllUsers(index, size, search, userType, cancellationToken);

        return Ok(new
        {
            success = true,
            users,
            totalCount,
            index,
            size
        });
    }

    /// <summary>
    /// Suspend a user account
    /// </summary>
    [HttpPost("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(
        string userId,
        [FromBody] SuspendUserRequest? request,
        CancellationToken cancellationToken)
    {
        await _userService.SuspendUser(userId, request?.Reason, cancellationToken);
        return Ok(new { success = true, message = "Kullanıcı hesabı donduruldu." });
    }

    /// <summary>
    /// Unsuspend a user account
    /// </summary>
    [HttpPost("users/{userId}/unsuspend")]
    public async Task<IActionResult> UnsuspendUser(
        string userId,
        CancellationToken cancellationToken)
    {
        await _userService.UnsuspendUser(userId, cancellationToken);
        return Ok(new { success = true, message = "Kullanıcı hesabı aktifleştirildi." });
    }
}

public class SuspendUserRequest
{
    public string? Reason { get; set; }
}
