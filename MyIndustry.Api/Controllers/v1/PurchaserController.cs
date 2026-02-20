using MediatR;
using MyIndustry.ApplicationService.Handler.Purchaser.CreatePurchaserCommand;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class PurchaserController : BaseController
{
    private readonly IMediator _mediator;

    public PurchaserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create new purchaser (internal use by Queue service)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaserCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
}
