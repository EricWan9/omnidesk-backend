using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OmniDesk.Api.Controllers.Contracts;
using OmniDesk.Api.Realtime.Conversations;
using OmniDesk.Api.Security;
using OmniDesk.Application.Attachments;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;
using System.ComponentModel.DataAnnotations;

namespace OmniDesk.Api.Controllers.Workspace;

[ApiController]
[Route("api/workspace/conversations")]
[EnableCors("AgentCors")]
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
    public async Task<ActionResult<GetConversationsResult>>
        GetConversations(
            [FromQuery] ConversationStatusFilter status = ConversationStatusFilter.All,
            [FromQuery] ConversationAssignmentFilter assignment = ConversationAssignmentFilter.All,
            [FromQuery] string? search = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
    {
        var tenantId = User.GetRequiredTenantId();
        var userId = User.GetRequiredUserId();

        var conversations =
            await _conversationService.GetConversationsAsync(
                tenantId,
                userId,
                page,
                pageSize,
                status,
                assignment,
                search,
                cancellationToken);

        return Ok(conversations);
    }

    [HttpPost("{conversationId:guid}/close")]
    public async Task<IActionResult> CloseConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var tenantId = User.GetRequiredTenantId();

        await _conversationService.CloseConversationAsync(
            tenantId,
            conversationId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{conversationId:guid}/reopen")]
    public async Task<IActionResult> ReopenConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var tenantId = User.GetRequiredTenantId();

        await _conversationService.ReopenConversationAsync(
            tenantId,
            conversationId,
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<ConversationDetailResponse>>
        GetConversation(
            Guid conversationId,
            CancellationToken cancellationToken)
    {
        var tenantId = User.GetRequiredTenantId();
        var userId = User.GetRequiredUserId();

        var conversation =
            await _conversationService.GetConversationAsync(
                tenantId,
                userId,
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

    [HttpPost("{conversationId:guid}/assign-to-me")]
    public async Task<IActionResult> AssignToMe(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var tenantId = User.GetRequiredTenantId();
        var userId = User.GetRequiredUserId();

        await _conversationService.AssignToMeAsync(
            tenantId,
            userId,
            conversationId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{conversationId:guid}/unassign")]
    public async Task<IActionResult> Unassign(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var tenantId = User.GetRequiredTenantId();

        await _conversationService.UnassignAsync(
            tenantId,
            conversationId,
            cancellationToken);

        return NoContent();
    }
}