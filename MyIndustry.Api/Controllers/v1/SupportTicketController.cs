using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;
using MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;
using MyIndustry.ApplicationService.Handler.SupportTicket.UpdateSupportTicketCommand;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/support-tickets")]
[ApiController]
public class SupportTicketController : BaseController
{
    private readonly IMediator _mediator;

    public SupportTicketController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new support ticket (public endpoint - no auth required)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        // Get user info if authenticated
        Guid? userId = null;
        int userType = 0; // Anonymous
        
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = GetUserId();
            // Determine user type from claims if available
            var typeClaim = User.FindFirst("type")?.Value;
            if (typeClaim != null)
            {
                if (typeClaim == "Seller" || typeClaim == "2")
                    userType = 2;
                else if (typeClaim == "Purchaser" || typeClaim == "1")
                    userType = 1;
            }
        }

        var command = new CreateSupportTicketCommand
        {
            UserId = userId,
            UserType = userType,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Subject = request.Subject,
            Message = request.Message,
            Category = request.Category
        };

        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Get all support tickets (admin only)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] int index = 1,
        [FromQuery] int size = 20,
        [FromQuery] TicketStatus? status = null,
        [FromQuery] TicketCategory? category = null,
        [FromQuery] TicketPriority? priority = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSupportTicketsQuery
        {
            Index = index,
            Size = size,
            Status = status,
            Category = category,
            Priority = priority
        };

        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Update a support ticket (admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSupportTicketCommand
        {
            Id = id,
            Status = request.Status,
            Priority = request.Priority,
            AdminNotes = request.AdminNotes,
            AdminResponse = request.AdminResponse
        };

        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
}

public class CreateTicketRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public TicketCategory Category { get; set; }
}

public class UpdateTicketRequest
{
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string? AdminNotes { get; set; }
    public string? AdminResponse { get; set; }
}
