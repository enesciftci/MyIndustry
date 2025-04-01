using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace MyIndustry.Api.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v{version:ApiVersion}/[controller]s")]
public class PurchaseController : BaseController
{
    private readonly IMediator _mediator;

    public PurchaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaserCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }
    
    [HttpPut]
    public async Task<IActionResult> Update(CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }
}