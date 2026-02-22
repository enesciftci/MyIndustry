using MediatR;
using Microsoft.AspNetCore.Authorization;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.CreateSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetSubscriptionPlanListQuery;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.UpdateSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.DeleteSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetAllSubscriptionPlansQuery;

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
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
       return CreateResponse(await _mediator.Send(new GetSubscriptionPlanListQuery(), cancellationToken));
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Unauthorized(new { success = false, message = "Bu işlem için admin yetkisi gereklidir." });
        }
        return CreateResponse(await _mediator.Send(new GetAllSubscriptionPlansQuery(), cancellationToken));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Unauthorized(new { success = false, message = "Bu işlem için admin yetkisi gereklidir." });
        }
        command.Id = id;
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteSubscriptionPlan(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Unauthorized(new { success = false, message = "Bu işlem için admin yetkisi gereklidir." });
        }
        return CreateResponse(await _mediator.Send(new DeleteSubscriptionPlanCommand { Id = id }, cancellationToken));
    }
}