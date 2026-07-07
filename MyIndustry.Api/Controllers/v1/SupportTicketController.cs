using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MyIndustry.ApplicationService.Handler.SupportTicket.CreateSupportTicketCommand;
using MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;
using MyIndustry.ApplicationService.Handler.SupportTicket.UpdateSupportTicketCommand;
using MyIndustry.Container.Extensions;
using MyIndustry.Container.Services;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Identity.Repository;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/support-tickets")]
[ApiController]
public class SupportTicketController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MyIndustryIdentityDbContext _identityDbContext;
    private readonly IRecaptchaVerificationService _recaptchaVerificationService;
    private readonly IConfiguration _configuration;

    public SupportTicketController(
        IMediator mediator,
        MyIndustryIdentityDbContext identityDbContext,
        IRecaptchaVerificationService recaptchaVerificationService,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _identityDbContext = identityDbContext;
        _recaptchaVerificationService = recaptchaVerificationService;
        _configuration = configuration;
    }

    /// <summary>
    /// Create a new support ticket (public endpoint - no auth required)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.SupportTicketPolicy)]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        if (request == null || !ModelState.IsValid)
            return BadRequest(ModelState);

        if (!string.IsNullOrWhiteSpace(_configuration["Recaptcha:SecretKey"]))
        {
            var captchaValid = await _recaptchaVerificationService.VerifyAsync(request.CaptchaToken, cancellationToken);
            if (!captchaValid)
                return BadRequest(new { success = false, message = "CAPTCHA doğrulaması başarısız." });
        }
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
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll(
        [FromQuery] [Range(1, 1000)] int index = 1,
        [FromQuery] [Range(1, 100)] int size = 20,
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
    [Authorize(Policy = "AdminOnly")]
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
    [Required(ErrorMessage = "Ad zorunludur")]
    [StringLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = "";

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Konu zorunludur")]
    [StringLength(500)]
    public string Subject { get; set; } = "";

    [Required(ErrorMessage = "Mesaj zorunludur")]
    [StringLength(4000)]
    public string Message { get; set; } = "";

    public TicketCategory Category { get; set; }

    public string? CaptchaToken { get; set; }
}

public class UpdateTicketRequest
{
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }

    [StringLength(2000)]
    public string? AdminNotes { get; set; }

    [StringLength(4000)]
    public string? AdminResponse { get; set; }
}
