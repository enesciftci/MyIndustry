using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.ApplicationService.Handler.UserLegalDocumentAcceptance.SaveUserLegalDocumentAcceptancesCommand;

namespace MyIndustry.Api.Controllers.v1;

/// <summary>
/// Identity servisi gibi dahili çağrılar için kullanılır. X-Internal-Api-Key header ile korunur.
/// </summary>
[ApiController]
[Route("api/v{version:ApiVersion}/[controller]")]
public class InternalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public InternalController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("user-legal-document-acceptances")]
    [AllowAnonymous]
    public async Task<IActionResult> SaveUserLegalDocumentAcceptances(
        [FromBody] SaveUserLegalDocumentAcceptancesRequest request,
        CancellationToken cancellationToken)
    {
        var expectedKey = _configuration["InternalApiKey"];
        if (string.IsNullOrEmpty(expectedKey) || Request.Headers["X-Internal-Api-Key"] != expectedKey)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveUserLegalDocumentAcceptancesCommand
        {
            UserId = request.UserId,
            LegalDocumentIds = request.LegalDocumentIds ?? new List<Guid>()
        }, cancellationToken);

        return Ok(result);
    }
}

public class SaveUserLegalDocumentAcceptancesRequest
{
    public Guid UserId { get; set; }
    public List<Guid>? LegalDocumentIds { get; set; }
}
