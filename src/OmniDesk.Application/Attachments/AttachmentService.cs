using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Application.Storage;

namespace OmniDesk.Application.Attachments;

public sealed class AttachmentService
    : IAttachmentService
{
    private readonly IMessageAttachmentRepository _messageAttachmentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IConversationAccessService _conversationAccessService;

    public AttachmentService(
        IMessageAttachmentRepository messageAttachmentRepository,
        IBlobStorageService blobStorageService,
        IConversationAccessService conversationAccessService)
    {
        _messageAttachmentRepository = messageAttachmentRepository;
        _blobStorageService = blobStorageService;
        _conversationAccessService = conversationAccessService;
    }

    public async Task<AttachmentDownloadResult?> GetForAgentAsync(
        Guid tenantId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment =
            await _messageAttachmentRepository
                .GetDownloadInfoAsync(
                    attachmentId,
                    cancellationToken);

        if (attachment is null ||
            attachment.TenantId != tenantId)
        {
            return null;
        }

        await _conversationAccessService
            .EnsureCanAccessAsync(
                tenantId,
                attachment.ConversationId,
                cancellationToken);

        var content =
            await _blobStorageService.OpenReadAsync(
                attachment.BlobName,
                cancellationToken);

        return new AttachmentDownloadResult(
            content,
            attachment.FileName,
            attachment.ContentType);
    }

    public async Task<AttachmentDownloadResult?> GetForCustomerAsync(
        Guid tenantId,
        Guid conversationId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment =
            await _messageAttachmentRepository
                .GetDownloadInfoAsync(
                    attachmentId,
                    cancellationToken);

        if (attachment is null ||
            attachment.TenantId != tenantId ||
            attachment.ConversationId != conversationId)
        {
            return null;
        }

        var content =
            await _blobStorageService.OpenReadAsync(
                attachment.BlobName,
                cancellationToken);

        return new AttachmentDownloadResult(
            content,
            attachment.FileName,
            attachment.ContentType);
    }
}