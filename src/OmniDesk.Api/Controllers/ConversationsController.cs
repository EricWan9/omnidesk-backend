using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OmniDesk.Application.Conversations;
using OmniDesk.Api.Hubs;
using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly IHubContext<ConversationHub> _hubContext;

    public ConversationsController(
        IConversationService conversationService,
        IHubContext<ConversationHub> hubContext)
    {
        _conversationService = conversationService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationListItemResponse>>>
        GetConversations(
            CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var conversations =
            await _conversationService.GetConversationsAsync(
                tenantId,
                cancellationToken);

        return Ok(conversations);
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<ConversationDetailResponse>>
        GetConversation(
            Guid conversationId,
            CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();

        var conversation =
            await _conversationService.GetConversationAsync(
                tenantId,
                conversationId,
                cancellationToken);

        if (conversation is null)
        {
            return NotFound();
        }

        return Ok(conversation);
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>>
        GetMessages(
            Guid conversationId,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();

        var messages =
            await _conversationService.GetMessagesAsync(
                tenantId,
                conversationId,
                pageSize,
                cancellationToken);

        return Ok(messages);
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<MessageResponse>>
        SendMessage(
            Guid conversationId,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var message =
            await _conversationService.SendMessageAsync(
                tenantId,
                userId,
                conversationId,
                request,
                cancellationToken);
        // Broadcast to SignalR clients subscribed to the conversation
        await _hubContext.Clients
            .Group(ConversationHub.GetConversationGroupName(conversationId))
            .SendAsync("MessageCreated", message, cancellationToken);

        return Created(
            $"/api/conversations/{conversationId}/messages/{message.Id}",
            message);
    }

    private Guid GetTenantId()
    {
        var value = User.FindFirstValue("tenantId");

        if (!Guid.TryParse(value, out var tenantId))
        {
            throw new UnauthorizedAccessException(
                "Tenant claim is missing or invalid.");
        }

        return tenantId;
    }

    private Guid GetUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("userId");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User claim is missing or invalid.");
        }

        return userId;
    }
}