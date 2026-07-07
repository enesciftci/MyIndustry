using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminListingsQuery;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminStatsQuery;
using MyIndustry.ApplicationService.Handler.Admin.SuspendSellerCommand;
using MyIndustry.ApplicationService.Handler.Admin.SuspendListingCommand;
using MyIndustry.Container.Logging;
using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize(Policy = "AdminOnly")]
public class AdminController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
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
        [FromQuery] [Range(1, 1000)] int index = 1,
        [FromQuery] [Range(1, 100)] int size = 20,
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
            RejectionReasonType = request.RejectionReasonType.HasValue 
                ? (RejectionReasonType?)request.RejectionReasonType.Value 
                : null,
            RejectionReasonDescription = request.RejectionReasonDescription
        };
        AdminAuditLogger.LogAdminAction(_logger, request.Approve ? "ApproveListing" : "RejectListing", GetUserId().ToString(), id.ToString(), HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString(), new { request.Approve });
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Suspend or unsuspend a listing
    /// </summary>
    [HttpPost("listings/{id}/suspend")]
    public async Task<IActionResult> SuspendListing(
        Guid id,
        [FromBody] SuspendRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SuspendListingCommand
        {
            ServiceId = id,
            Suspend = request.Suspend,
            SuspensionReasonType = request.SuspensionReasonType.HasValue 
                ? (SuspensionReasonType?)request.SuspensionReasonType.Value 
                : null,
            SuspensionReasonDescription = request.SuspensionReasonDescription
        };
        AdminAuditLogger.LogAdminAction(_logger, request.Suspend ? "SuspendListing" : "UnsuspendListing", GetUserId().ToString(), id.ToString(), HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString());
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Suspend or unsuspend a seller
    /// </summary>
    [HttpPost("sellers/{id}/suspend")]
    public async Task<IActionResult> SuspendSeller(
        Guid id,
        [FromBody] SuspendRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SuspendSellerCommand
        {
            SellerId = id,
            Suspend = request.Suspend,
            Reason = request.SuspensionReasonDescription // Map to Reason for backward compatibility
        };
        AdminAuditLogger.LogAdminAction(_logger, request.Suspend ? "SuspendSeller" : "UnsuspendSeller", GetUserId().ToString(), id.ToString(), HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString());
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
}

public class ApproveListingRequest
{
    public bool Approve { get; set; }
    public int? RejectionReasonType { get; set; }

    [StringLength(1000)]
    public string? RejectionReasonDescription { get; set; }
}

public class SuspendRequest
{
    public bool Suspend { get; set; }
    public int? SuspensionReasonType { get; set; }

    [StringLength(1000)]
    public string? SuspensionReasonDescription { get; set; }
}
