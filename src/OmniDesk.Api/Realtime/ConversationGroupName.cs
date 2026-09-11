namespace OmniDesk.Api.Realtime;

internal static class ConversationGroupName
{
    public static string ForConversation(
        Guid tenantId,
        Guid conversationId)
    {
        return $"tenant:{tenantId}:conversation:{conversationId}";
    }
}
