using OmniDesk.Domain.Conversations.Enums;

namespace OmniDesk.Application.Conversations.Models;

public sealed record MessageResponse(
    Guid Id,
    Guid ConversationId,
    MessageSenderResponse MessageSender,
    string Content,
    DateTime CreatedAt
);

public sealed record MessageSenderResponse(
    MessageSenderType Type,
    Guid? Id
);