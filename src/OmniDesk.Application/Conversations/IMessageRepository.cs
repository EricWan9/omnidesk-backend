using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations;

namespace OmniDesk.Application.Conversations;

public interface IMessageRepository
{
    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid tenantId,
        Guid conversationId,
        int pageSize,
        CancellationToken cancellationToken);

    void AddMessage(Message message);
}