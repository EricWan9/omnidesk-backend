namespace OmniDesk.Application.Attachments;

public sealed record AttachmentUpload(
    string FileName,
    string ContentType,
    long Length,
    Stream Content);