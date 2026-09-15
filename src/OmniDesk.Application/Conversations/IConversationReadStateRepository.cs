using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations;

namespace OmniDesk.Application.Conversations;

public interface IConversationReadStateRepository
{
    Task<ConversationReadState?> GetAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken);

    void Add(ConversationReadState readState);
}
