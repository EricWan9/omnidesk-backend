using OmniDesk.Domain.Conversations.Enums;

namespace OmniDesk.Application.Conversations.Models;

public sealed record ConversationDetailResponse(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    ConversationStatus Status,
    Guid? AssignedUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    byte[] RowVersion
);