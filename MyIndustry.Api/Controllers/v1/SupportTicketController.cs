using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;
using MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;
using MyIndustry.ApplicationService.Handler.SupportTicket.UpdateSupportTicketCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Identity.Repository;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/support-tickets")]
[ApiController]
public class SupportTicketController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MyIndustryIdentityDbContext _identityDbContext;

    public SupportTicketController(IMediator mediator, MyIndustryIdentityDbContext identityDbContext)
    {
        _mediator = mediator;
        _identityDbContext = identityDbContext;
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
        string name = request.Name;
        string email = request.Email;
        string? phone = request.Phone;
        
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = GetUserId();
            
            // Get user information from Identity database
            var user = await _identityDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.ToString(), cancellationToken);
            
            if (user != null)
            {
                // Use database values instead of request values for logged-in users
                name = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(name))
                    name = user.Email ?? request.Name;
                
                email = user.Email ?? request.Email;
                phone = user.PhoneNumber ?? request.Phone;
                
                // Determine user type from database
                var parsedUserType = (int)user.Type;
                // UserType enum: 0=User, 1=Purchaser, 2=Seller, 99=Admin
                // For support tickets, we map: 0=User (Alıcı), 1=Purchaser (Alıcı), 2=Seller (Satıcı)
                if (parsedUserType == 0 || parsedUserType == 1)
                    userType = 1; // Alıcı (User or Purchaser)
                else if (parsedUserType == 2)
                    userType = 2; // Satıcı (Seller)
                // Admin (99) is not a regular user type for support tickets, keep as 0
            }
        }

        var command = new CreateSupportTicketCommand
        {
            UserId = userId,
            UserType = userType,
            Name = name,
            Email = email,
            Phone = phone,
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
