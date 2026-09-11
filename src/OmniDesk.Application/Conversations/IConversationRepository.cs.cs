using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations.Entities;
using OmniDesk.Domain.Entities;

namespace OmniDesk.Application.Conversations;

public interface IConversationRepository
{
    Task<IReadOnlyList<ConversationListItemResponse>> GetConversationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken);

    Task<ConversationDetailResponse?> GetConversationByIdAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid tenantId,
        Guid conversationId,
        int pageSize,
        CancellationToken cancellationToken);

    Task AddMessageAsync(
        Message message,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}