namespace OmniDesk.Application.Conversations.Exceptions;

public sealed class ConversationClosedException : Exception
{
    public ConversationClosedException(Guid conversationId)
        : base($"Conversation '{conversationId}' is closed and cannot accept new messages.")
    {
    }
}
