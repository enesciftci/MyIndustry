using MediatR;
using MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;
using MyIndustry.ApplicationService.Handler.Service.DisableServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;
using MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class ServiceController : BaseController
{
    private readonly IMediator _mediator;

    public ServiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceCommand command,
        CancellationToken cancellationToken)
    {
        command.SellerId = GetUserId();
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetServicesBySellerId([FromQuery] GetServicesBySellerIdQuery query,
        CancellationToken cancellationToken)
    {
        query.SellerId = GetUserId();
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("list/randomly")]
    public async Task<IActionResult> GetServicesByRandomly([FromQuery] GetServicesByRandomlyQuery query,
        CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpPut("{serviceId:guid}")]
    public async Task<IActionResult> Update(Guid serviceId, UpdateServiceByIdCommand command, CancellationToken cancellationToken)
    {
        command.ServiceDto.Id = serviceId;
        command.ServiceDto.SellerId = GetUserId();
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetServiceById(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetServicesByIdQuery() {Id = id}, cancellationToken));
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetServiceByCategory([FromQuery] GetServicesByFilterQuery command, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> DisableServiceById(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new DisableServiceByIdCommand()
        {
            SellerId = GetUserId(),
            ServiceId = id
        }, cancellationToken));
    }
}