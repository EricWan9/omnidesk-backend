using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OmniDesk.Api.Realtime.Conversations;
using OmniDesk.Api.Security;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;
using System.ComponentModel.DataAnnotations;

namespace OmniDesk.Api.Controllers.Workspace;

[ApiController]
[Route("api/workspace/conversations")]
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
        var tenantId = User.GetRequiredTenantId();

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
        var tenantId = User.GetRequiredTenantId();

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
            [FromQuery, Range(1, 100)] int pageSize = 50,
            CancellationToken cancellationToken = default)
    {
        var tenantId = User.GetRequiredTenantId();

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
        var tenantId = User.GetRequiredTenantId();
        var userId = User.GetRequiredUserId();

        var command = new SendMessageCommand(
            TenantId: tenantId,
            ConversationId: conversationId,
            MessageSender: MessageSender.Agent(userId),
            Content: request.Content);

        var message =
            await _conversationService.SendMessageAsync(
                command,
                cancellationToken);

        return Created(
            $"/api/conversations/{conversationId}/messages/{message.Id}",
            message);
    }
}