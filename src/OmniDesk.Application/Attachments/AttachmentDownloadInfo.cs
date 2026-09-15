namespace OmniDesk.Application.Attachments;

public sealed record AttachmentDownloadInfo(
    Guid AttachmentId,
    Guid ConversationId,
    Guid TenantId,
    string BlobName,
    string FileName,
    string ContentType);