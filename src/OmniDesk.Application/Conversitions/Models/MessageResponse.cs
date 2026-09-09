namespace OmniDesk.Application.Conversations.Models;

public sealed record MessageResponse(
    Guid Id,
    Guid ConversationId,
    MessageSenderType SenderType,
    Guid? SenderUserId,
    string Content,
    DateTime CreatedAt
);