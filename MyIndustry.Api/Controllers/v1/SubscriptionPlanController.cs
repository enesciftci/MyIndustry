using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.CreateSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetSubscriptionPlanListQuery;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.UpdateSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.DeleteSubscriptionPlanCommand;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetAllSubscriptionPlansQuery;
using MyIndustry.Container.Logging;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class SubscriptionPlanController(IMediator mediator, ILogger<SubscriptionPlanController> logger) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<SubscriptionPlanController> _logger = logger;

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        AdminAuditLogger.LogAdminAction(_logger, "CreateSubscriptionPlan", GetUserId().ToString(), command.Name,
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString(), new { command.Name });
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
       return CreateResponse(await _mediator.Send(new GetSubscriptionPlanListQuery(), cancellationToken));
    }

    [HttpGet("all")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetAllSubscriptionPlansQuery(), cancellationToken));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        AdminAuditLogger.LogAdminAction(_logger, "UpdateSubscriptionPlan", GetUserId().ToString(), id.ToString(),
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString(), new { command.Name });
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteSubscriptionPlan(Guid id, CancellationToken cancellationToken)
    {
        AdminAuditLogger.LogAdminAction(_logger, "DeleteSubscriptionPlan", GetUserId().ToString(), id.ToString(),
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString());
        return CreateResponse(await _mediator.Send(new DeleteSubscriptionPlanCommand { Id = id }, cancellationToken));
    }
}
