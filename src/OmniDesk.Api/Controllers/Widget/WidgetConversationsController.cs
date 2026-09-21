using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OmniDesk.Api.Controllers.Contracts;
using OmniDesk.Api.Security;
using OmniDesk.Application.Attachments;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Application.Widgets;
using OmniDesk.Application.Widgets.Models;

namespace OmniDesk.Api.Controllers.Widget;

[ApiController]
[Authorize]
[Route("api/widget/conversations")]
[EnableCors("WidgetCors")]
public sealed class WidgetConversationsController : ControllerBase
{
    private readonly IWidgetConversationService _widgetConversationService;
    private readonly IConversationService _conversationService;

    public WidgetConversationsController(
        IWidgetConversationService widgetConversationService,
        IConversationService conversationService)
    {
        _widgetConversationService = widgetConversationService;
        _conversationService = conversationService;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<StartWidgetConversationResponse>>
        StartConversation(
            StartWidgetConversationRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await _widgetConversationService.StartConversationAsync(
                new StartWidgetConversationCommand(
                    request.WidgetKey),
                cancellationToken);

        var response =
            new StartWidgetConversationResponse(
                result.ConversationId,
                result.AccessToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>>
        GetMessages(
            Guid conversationId,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
    {
        User.EnsureCustomerActor();

        var tenantId = User.GetRequiredTenantId();
        var authorizedConversationId =
            User.GetRequiredConversationId();

        EnsureConversationAccess(
            conversationId,
            authorizedConversationId);

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

        var customerId =
            User.GetRequiredCustomerId();

        var authorizedConversationId =
            User.GetRequiredConversationId();

        if (conversationId != authorizedConversationId)
        {
            return Forbid();
        }

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
                    MessageSender.Customer(customerId),
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

    private static void EnsureConversationAccess(
        Guid requestedConversationId,
        Guid authorizedConversationId)
    {
        if (requestedConversationId != authorizedConversationId)
        {
            throw new UnauthorizedAccessException(
                "The customer cannot access this conversation.");
        }
    }
}