using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminListingsQuery;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminStatsQuery;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize] // TODO: Add admin role check
public class AdminController : BaseController
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetAdminStatsQuery(), cancellationToken));
    }

    /// <summary>
    /// Get all listings for admin with filtering
    /// </summary>
    [HttpGet("listings")]
    public async Task<IActionResult> GetListings(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int index = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminListingsQuery
        {
            Status = status,
            SearchTerm = search,
            Index = index,
            Size = size
        };
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Approve or reject a listing
    /// </summary>
    [HttpPost("listings/{id}/approve")]
    public async Task<IActionResult> ApproveListing(
        Guid id,
        [FromBody] ApproveListingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveListingCommand
        {
            ServiceId = id,
            Approve = request.Approve,
            RejectionReason = request.RejectionReason
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
}

public class ApproveListingRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}
