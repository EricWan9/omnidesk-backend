namespace OmniDesk.Application.Conversations.Models;

public sealed record GetConversationsResult(
    IReadOnlyList<ConversationListItemResponse> Conversations,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
