using MediatR;
using Microsoft.AspNetCore.Authorization;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerProfileQuery;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerCommand;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerProfileCommand;
using MyIndustry.ApplicationService.Handler.Verification.SendEmailChangeVerificationCommand;
using MyIndustry.ApplicationService.Handler.Verification.SendPhoneVerificationCommand;
using MyIndustry.ApplicationService.Handler.Verification.VerifyEmailChangeCommand;
using MyIndustry.ApplicationService.Handler.Verification.VerifyPhoneCommand;

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
    /// Update seller profile (basic info, not phone/email)
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateSellerProfileCommand command, CancellationToken cancellationToken)
    {
        command = command with { UserId = GetUserId() };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Send phone verification code
    /// </summary>
    [HttpPost("phone/send-verification")]
    [Authorize]
    public async Task<IActionResult> SendPhoneVerification([FromBody] SendPhoneVerificationRequest request, CancellationToken cancellationToken)
    {
        var command = new SendPhoneVerificationCommand
        {
            UserId = GetUserId(),
            PhoneNumber = request.PhoneNumber
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Verify phone number with code
    /// </summary>
    [HttpPost("phone/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyPhoneCommand
        {
            UserId = GetUserId(),
            PhoneNumber = request.PhoneNumber,
            Code = request.Code
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Send email change verification code
    /// </summary>
    [HttpPost("email/send-verification")]
    [Authorize]
    public async Task<IActionResult> SendEmailVerification([FromBody] SendEmailVerificationRequest request, CancellationToken cancellationToken)
    {
        var command = new SendEmailChangeVerificationCommand
        {
            UserId = GetUserId(),
            NewEmail = request.NewEmail
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Verify email change with code
    /// </summary>
    [HttpPost("email/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyEmailChangeCommand
        {
            UserId = GetUserId(),
            NewEmail = request.NewEmail,
            Code = request.Code
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }
}

// Request DTOs
public record SendPhoneVerificationRequest(string PhoneNumber);
public record VerifyPhoneRequest(string PhoneNumber, string Code);
public record SendEmailVerificationRequest(string NewEmail);
public record VerifyEmailRequest(string NewEmail, string Code);