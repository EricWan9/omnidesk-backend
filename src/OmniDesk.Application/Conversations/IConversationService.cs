using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Application.Conversations;

public interface IConversationService
{
    Task<GetConversationsResult> GetConversationsAsync(
        Guid tenantId,
        Guid userId,
        int page,
        int pageSize,
        ConversationStatusFilter statusFilter,
        ConversationAssignmentFilter assignmentFilter,
        string? search,
        CancellationToken cancellationToken = default);

    Task<ConversationListItemResponse?> GetConversationAsync(
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid tenantId,
        Guid conversationId,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<MessageResponse> SendMessageAsync(
        SendMessageCommand command,
        CancellationToken cancellationToken);

    Task MarkAsReadAsync(
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken);

    Task CloseConversationAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);

    Task ReopenConversationAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);

    Task AssignToMeAsync(
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken);

    Task UnassignAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);
}