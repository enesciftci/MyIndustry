using MediatR;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerCommand;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class SellerController : BaseController
{
    private readonly IMediator _mediator;

    public SellerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSellerCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("list")]
    public async Task<IActionResult> Get([FromQuery] int index,[FromQuery] int size, CancellationToken cancellationToken)
    {
        var query = new GetSellerListQuery()
        {
            Pager = new Pager(index, size)
        };
        
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSellerCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }
}