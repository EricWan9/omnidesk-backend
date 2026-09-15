using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Application.Attachments;

public interface IAttachmentService
{
    Task<AttachmentDownloadResult?> GetForAgentAsync(
        Guid tenantId,
        Guid attachmentId,
        CancellationToken cancellationToken);

    Task<AttachmentDownloadResult?> GetForCustomerAsync(
        Guid tenantId,
        Guid conversationId,
        Guid attachmentId,
        CancellationToken cancellationToken);
}