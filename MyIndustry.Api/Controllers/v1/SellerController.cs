using MediatR;
using Microsoft.AspNetCore.Authorization;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerByIdQuery;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerProfileQuery;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerCommand;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerProfileCommand;

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

    /// <summary>
    /// Get seller by ID (public)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSellerByIdQuery { SellerId = id };
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSellerCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }

    /// <summary>
    /// Get current seller's profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var query = new GetSellerProfileQuery
        {
            UserId = GetUserId()
        };
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Update seller profile (basic info)
    /// Note: Phone/Email verification is handled by Identity API
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateSellerProfileCommand command, CancellationToken cancellationToken)
    {
        command = command with { UserId = GetUserId() };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
}