using OmniDesk.Domain.Conversations;

namespace OmniDesk.Application.Conversations.Models;

public sealed record MessageResponse(
    Guid Id,
    Guid ConversationId,
    MessageSenderResponse MessageSender,
    string? Content,
    DateTime CreatedAt,
    IReadOnlyList<MessageAttachmentResponse> Attachments
);

public sealed record MessageSenderResponse(
    MessageSenderType Type,
    Guid? Id
);

public sealed record MessageAttachmentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Size);