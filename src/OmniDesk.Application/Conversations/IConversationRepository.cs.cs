using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations;

namespace OmniDesk.Application.Conversations;

public interface IConversationRepository
{
    Task<IReadOnlyList<ConversationListItemResponse>> GetConversationsAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<ConversationDetailResponse?> GetConversationDetailByIdAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);

    Task<Conversation?> GetConversationByIdAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);

    void AddConversation(Conversation conversation);
}