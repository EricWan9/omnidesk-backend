using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OmniDesk.Api.Hubs;

[Authorize]
public class ConversationHub : Hub
{
    public Task JoinConversation(Guid conversationId)
    {
        return Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetConversationGroupName(conversationId));
    }

    public Task LeaveConversation(Guid conversationId)
    {
        return Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetConversationGroupName(conversationId));
    }

    public static string GetConversationGroupName(Guid conversationId)
    {
        return $"conversation:{conversationId}";
    }
}