using MediatR;
using Microsoft.AspNetCore.Authorization;
using MyIndustry.ApplicationService.Handler.Purchaser.GetPurchaserQuery;

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreateResponse(response);
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(
            new GetPurchaserQuery() { PurchaserId = GetUserId() }, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update(CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }
}