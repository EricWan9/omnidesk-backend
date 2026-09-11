namespace OmniDesk.Api.Realtime.Conversations;

internal static class ConversationGroupName
{
    public static string ForConversation(
        Guid tenantId,
        Guid conversationId)
        => $"tenant:{tenantId}:conversation:{conversationId}";

    public static string ForWorkspace(Guid tenantId)
        => $"tenant:{tenantId}:workspace";
}
