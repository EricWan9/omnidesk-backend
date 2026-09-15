using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniDesk.Api.Security;
using OmniDesk.Application.Attachments;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Identity;
using OmniDesk.Domain.Security;

namespace OmniDesk.Api.Controllers.Common;

[ApiController]
[Authorize]
[Route("api/attachments")]
public sealed class AttachmentsController
    : ControllerBase
{
    private readonly IAttachmentService
        _attachmentService;

    public AttachmentsController(
        IAttachmentService attachmentService)
    {
        _attachmentService =
            attachmentService;
    }

    [HttpGet("{attachmentId:guid}")]
    public async Task<IActionResult> GetAttachment(
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var actorType =
            User.GetRequiredActorType();

        var tenantId =
            User.GetRequiredTenantId();

        AttachmentDownloadResult? result;

        switch (actorType)
        {
            case OmniDeskActorTypes.Agent:
                result =
                    await _attachmentService.GetForAgentAsync(
                        tenantId,
                        attachmentId,
                        cancellationToken);

                break;

            case OmniDeskActorTypes.Customer:
                var conversationId =
                    User.GetRequiredConversationId();

                result =
                    await _attachmentService.GetForCustomerAsync(
                        tenantId,
                        conversationId,
                        attachmentId,
                        cancellationToken);

                break;

            default:
                return Forbid();
        }

        if (result is null)
        {
            return NotFound();
        }

        return File(
            result.Content,
            result.ContentType,
            result.FileName,
            enableRangeProcessing: true);
    }
}
