using OmniDesk.Application.Conversations.Exceptions;

namespace OmniDesk.Application.Conversations;

public sealed class ConversationAccessService
    : IConversationAccessService
{
    private readonly IConversationRepository _conversationRepository;

    public ConversationAccessService(
        IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task EnsureCanAccessAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var conversation =
            await _conversationRepository.GetConversationByIdAsync(
                tenantId,
                conversationId,
                cancellationToken);

        if (conversation is null)
        {
            throw new ConversationNotFoundException(conversationId);
        }
    }
}