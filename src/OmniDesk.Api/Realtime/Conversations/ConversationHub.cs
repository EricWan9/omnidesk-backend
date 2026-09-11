using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmniDesk.Api.Security;
using OmniDesk.Application.Conversations;

namespace OmniDesk.Api.Realtime.Conversations;

[Authorize]
public sealed class ConversationHub : Hub<IConversationClient>
{
    private readonly IConversationAccessService _conversationAccessService;

    public ConversationHub(
        IConversationAccessService conversationAccessService)
    {
        _conversationAccessService = conversationAccessService;
    }

    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User!.GetRequiredTenantId();

        var workspaceGroupName =
            ConversationGroupName.ForWorkspace(tenantId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            workspaceGroupName,
            Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    public async Task SubscribeConversation(
        Guid conversationId)
    {
        var tenantId = Context.User!.GetRequiredTenantId();

        await _conversationAccessService.EnsureCanAccessAsync(
            tenantId,
            conversationId,
            Context.ConnectionAborted);

        var groupName =
            ConversationGroupName.ForConversation(
                tenantId,
                conversationId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            groupName,
            Context.ConnectionAborted);
    }

    public async Task UnsubscribeConversation(
        Guid conversationId)
    {
        var tenantId = Context.User!.GetRequiredTenantId();

        var groupName =
            ConversationGroupName.ForConversation(
                tenantId,
                conversationId);

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            groupName,
            Context.ConnectionAborted);
    }
}