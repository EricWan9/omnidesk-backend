using OmniDesk.Application.Attachments;
using OmniDesk.Domain.Storage;

namespace OmniDesk.Application.Conversations;

public interface IMessageAttachmentRepository
{
    void Add(MessageAttachment attachment);

    Task<AttachmentDownloadInfo?> GetDownloadInfoAsync(
        Guid attachmentId,
        CancellationToken cancellationToken);
}