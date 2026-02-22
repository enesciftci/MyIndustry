using MediatR;
using MyIndustry.ApplicationService.Handler.SellerSubscription.CreateSellerSubscriptionCommand;
using MyIndustry.ApplicationService.Handler.SellerSubscription.GetSellerSubscriptionQuery;
using MyIndustry.ApplicationService.Handler.SellerSubscription.UpgradeSellerSubscriptionCommand;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class SellerSubscriptionController : BaseController
{
    private readonly IMediator _mediator;

    public SellerSubscriptionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSellerSubscription(CancellationToken cancellationToken)
    {
       return CreateResponse(await _mediator.Send(new GetSellerSubscriptionQuery()
       {
           SellerId = GetUserId()
       }, cancellationToken));
    }
    
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] CreateSellerSubscriptionCommand sellerSubscriptionCommand, CancellationToken cancellationToken)
    {
        sellerSubscriptionCommand.SellerId = GetUserId();
        return CreateResponse(await _mediator.Send(sellerSubscriptionCommand, cancellationToken));
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] UpgradeSellerSubscriptionCommand upgradeCommand, CancellationToken cancellationToken)
    {
        upgradeCommand.SellerId = GetUserId();
        return CreateResponse(await _mediator.Send(upgradeCommand, cancellationToken));
    }
}