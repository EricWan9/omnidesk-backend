using OmniDesk.Domain.Conversations;

namespace OmniDesk.Application.Conversations.Models;

public sealed record ConversationListItemResponse(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    ConversationStatus Status,
    Guid? AssignedUserId,
    string? LastMessage,
    DateTime? LastMessageAt,
    DateTime UpdatedAt,
    byte[] RowVersion
);