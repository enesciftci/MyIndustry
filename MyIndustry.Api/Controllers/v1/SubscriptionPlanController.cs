using MediatR;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.CreateSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetSubscriptionPlanListQuery;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class SubscriptionPlanController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        return CreateResponse( await _mediator.Send(command, cancellationToken));
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
       return CreateResponse( await _mediator.Send(new GetSubscriptionPlanListQuery(), cancellationToken));
    }
}