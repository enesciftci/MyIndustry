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
}
