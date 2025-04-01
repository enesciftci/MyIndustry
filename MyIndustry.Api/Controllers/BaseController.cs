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
}