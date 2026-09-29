using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations;

namespace OmniDesk.Application.Conversations;

public interface IConversationRepository
{
    Task<GetConversationsResult> GetConversationsAsync(
        Guid tenantId,
        Guid userId,
        int page,
        int pageSize,
        ConversationStatusFilter statusFilter,
        ConversationAssignmentFilter assignmentFilter,
        string? search,
        CancellationToken cancellationToken);

    Task<ConversationListItemResponse?> GetConversationDetailByIdAsync(
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken);

    Task<Conversation?> GetConversationByIdAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);

    void AddConversation(Conversation conversation);
}