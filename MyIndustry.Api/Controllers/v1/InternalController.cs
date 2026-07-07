using System.Security.Cryptography;
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
        var providedKey = Request.Headers["X-Internal-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(expectedKey) ||
            string.IsNullOrEmpty(providedKey) ||
            !CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(expectedKey),
                System.Text.Encoding.UTF8.GetBytes(providedKey)))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveUserLegalDocumentAcceptancesCommand
        {
            UserId = request.UserId,
            LegalDocumentIds = (request.LegalDocumentIds ?? new List<Guid>()).Take(100).ToList()
        }, cancellationToken);

        return Ok(result);
    }
}

public class SaveUserLegalDocumentAcceptancesRequest
{
    public Guid UserId { get; set; }
    public List<Guid>? LegalDocumentIds { get; set; }
}
