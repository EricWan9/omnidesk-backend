using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OmniDesk.Api.Controllers.Contracts;
using OmniDesk.Api.Realtime.Conversations;
using OmniDesk.Api.Security;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Attachments;
using OmniDesk.Application.Conversations.Models;
using System.ComponentModel.DataAnnotations;

namespace OmniDesk.Api.Controllers.Workspace;

[ApiController]
[Route("api/workspace/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationsController(
        IConversationService conversationService,
        IHubContext<ConversationHub> hubContext)
    {
        _conversationService = conversationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationListItemResponse>>>
        GetConversations(
            CancellationToken cancellationToken)
    {
        var tenantId = User.GetRequiredTenantId();
        var userId = User.GetRequiredUserId();

        var conversations =
            await _conversationService.GetConversationsAsync(
                tenantId,
                userId,
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
    public async Task<ActionResult<MessageResponse>> SendMessage(
        Guid conversationId,
        [FromForm] SendMessageForm form,
        CancellationToken cancellationToken)
    {
        var tenantId =
            User.GetRequiredTenantId();

        var userId =
            User.GetRequiredUserId();

        var uploads = form.Files
            .Select(file =>
                new AttachmentUpload(
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    file.OpenReadStream()))
            .ToList();

        try
        {
            var command =
                new SendMessageCommand(
                    tenantId,
                    conversationId,
                    MessageSender.Agent(userId),
                    form.Content,
                    uploads);

            var response =
                await _conversationService.SendMessageAsync(
                    command,
                    cancellationToken);

            return Ok(response);
        }
        finally
        {
            foreach (var upload in uploads)
            {
                await upload.Content.DisposeAsync();
            }
        }
    }

    [HttpPost("{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var tenantId =
            User.GetRequiredTenantId();

        var userId =
            User.GetRequiredUserId();

        await _conversationService.MarkAsReadAsync(
            tenantId,
            userId,
            conversationId,
            cancellationToken);

        return NoContent();
    }
}