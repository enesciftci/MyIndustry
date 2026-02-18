using MyIndustry.ApplicationService.Handler;

namespace MyIndustry.Api.Controllers;

[Produces("application/json")]
public class BaseController: ControllerBase
{
    /// <summary>
    /// This method creates a http response according to the response object Success value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="responseObject"></param>
    /// <returns></returns>
    protected virtual IActionResult CreateResponse(object responseObject)
    {
        if (((ResponseBase)responseObject).Success)
            return new OkObjectResult(responseObject);
        return new BadRequestObjectResult(responseObject);
    }

    protected Guid GetUserId()
    {
        if (Guid.TryParse(User.Claims.FirstOrDefault(p => p.Type == "uid")?.Value, out Guid userId))
            return userId;

        throw new UnauthorizedAccessException();
    }

    protected string GetUserEmail()
    {
        return User.Claims.FirstOrDefault(p => p.Type == "email")?.Value ?? "";
    }

    protected string GetUserName()
    {
        var firstName = User.Claims.FirstOrDefault(p => p.Type == "given_name")?.Value ?? "";
        var lastName = User.Claims.FirstOrDefault(p => p.Type == "family_name")?.Value ?? "";
        
        if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            return $"{firstName} {lastName}".Trim();
        
        // Fallback to email if name not available
        return GetUserEmail().Split('@').FirstOrDefault() ?? "Kullanıcı";
    }
}