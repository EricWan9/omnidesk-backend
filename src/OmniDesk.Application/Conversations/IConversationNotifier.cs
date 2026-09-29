using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Application.Conversations;

public interface IConversationNotifier
{
    Task MessageSentAsync(
        Guid tenantId,
        Guid conversationId,
        MessageResponse message,
        CancellationToken cancellationToken);

    Task ConversationUpdatedAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);
}