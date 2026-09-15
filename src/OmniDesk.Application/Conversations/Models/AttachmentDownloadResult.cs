namespace OmniDesk.Application.Conversations.Models;

public sealed record AttachmentDownloadResult(
    Stream Content,
    string FileName,
    string ContentType);
