using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmniDesk.Api.Security;
using OmniDesk.Application.Conversations;

namespace OmniDesk.Api.Hubs;

[Authorize]
public class ConversationHub : Hub
{
    private readonly IConversationService _conversationService;

    public ConversationHub(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var user = Context.User 
            ?? throw new HubException("User is not authenticated.");

        var tenantId = user.GetRequiredTenantId();
        var conversation =
            await _conversationService.GetConversationAsync(
                tenantId,
                conversationId,
                Context.ConnectionAborted);

        if (conversation is null)
        {
            throw new HubException(
                "Conversation not found or access denied.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetConversationGroupName(conversationId),
            Context.ConnectionAborted);
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetConversationGroupName(conversationId),
            Context.ConnectionAborted);
    }

    public static string GetConversationGroupName(Guid conversationId)
    {
        return $"conversation:{conversationId}";
    }
}