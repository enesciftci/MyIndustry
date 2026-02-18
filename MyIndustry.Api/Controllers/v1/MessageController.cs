using MediatR;
using Microsoft.AspNetCore.Authorization;
using MyIndustry.ApplicationService.Handler.Message.GetConversationMessagesQuery;
using MyIndustry.ApplicationService.Handler.Message.GetConversationsQuery;
using MyIndustry.ApplicationService.Handler.Message.GetUnreadCountQuery;
using MyIndustry.ApplicationService.Handler.Message.MarkMessagesAsReadCommand;
using MyIndustry.ApplicationService.Handler.Message.ReplyMessageCommand;
using MyIndustry.ApplicationService.Handler.Message.SendMessageCommand;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class MessageController : BaseController
{
    private readonly IMediator _mediator;

    public MessageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Send a new message to a seller about a service
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var command = new SendMessageCommand
        {
            ServiceId = request.ServiceId,
            SenderId = GetUserId(),
            SenderName = GetUserName(),
            SenderEmail = GetUserEmail(),
            Content = request.Content
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Get all conversations for the current user
    /// </summary>
    [HttpGet("conversations")]
    [Authorize]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
    {
        var query = new GetConversationsQuery
        {
            UserId = GetUserId()
        };
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Get messages in a specific conversation
    /// </summary>
    [HttpGet("conversation/{serviceId}/{otherUserId}")]
    [Authorize]
    public async Task<IActionResult> GetConversationMessages(Guid serviceId, Guid otherUserId, CancellationToken cancellationToken)
    {
        var query = new GetConversationMessagesQuery
        {
            UserId = GetUserId(),
            ServiceId = serviceId,
            OtherUserId = otherUserId
        };
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>
    /// Reply to a conversation
    /// </summary>
    [HttpPost("reply")]
    [Authorize]
    public async Task<IActionResult> ReplyMessage([FromBody] ReplyMessageRequest request, CancellationToken cancellationToken)
    {
        var command = new ReplyMessageCommand
        {
            UserId = GetUserId(),
            UserName = GetUserName(),
            UserEmail = GetUserEmail(),
            ServiceId = request.ServiceId,
            ReceiverId = request.ReceiverId,
            Content = request.Content
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Mark messages in a conversation as read
    /// </summary>
    [HttpPut("read/{serviceId}/{otherUserId}")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(Guid serviceId, Guid otherUserId, CancellationToken cancellationToken)
    {
        var command = new MarkMessagesAsReadCommand
        {
            UserId = GetUserId(),
            ServiceId = serviceId,
            OtherUserId = otherUserId
        };
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Get unread message count for badge
    /// </summary>
    [HttpGet("unread-count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var query = new GetUnreadCountQuery
        {
            UserId = GetUserId()
        };
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }
}

// Request DTOs
public record SendMessageRequest(Guid ServiceId, string Content);
public record ReplyMessageRequest(Guid ServiceId, Guid ReceiverId, string Content);
