using OmniDesk.Domain.Conversations;

namespace OmniDesk.Api.Controllers.Contracts;

public sealed record ConversationUpdatedMessage(
    Guid ConversationId,
    MessageSenderType SenderType,
    string? LastMessage,
    DateTime LastMessageAt);