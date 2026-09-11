namespace OmniDesk.Application.Conversations.Exceptions;

public sealed class ConversationNotFoundException : Exception
{
    public ConversationNotFoundException(Guid conversationId)
        : base($"Conversation '{conversationId}' was not found.")
    {
    }
}
