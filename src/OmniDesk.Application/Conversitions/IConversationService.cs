using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Application.Conversations;

public interface IConversationService
{
    Task<IReadOnlyList<ConversationListItemResponse>> GetConversationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<ConversationDetailResponse?> GetConversationAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid tenantId,
        Guid conversationId,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<MessageResponse> SendMessageAsync(
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        SendMessageRequest request,
        CancellationToken cancellationToken = default);
}