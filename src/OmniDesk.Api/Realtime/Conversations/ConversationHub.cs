using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmniDesk.Api.Security;
using OmniDesk.Application.Conversations;
using OmniDesk.Domain.Security;

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
        var user = Context.User
            ?? throw new HubException(
                "Unauthenticated connection.");

        var actorType =
            user.GetRequiredActorType();

        if (actorType == OmniDeskActorTypes.Agent)
        {
            var tenantId =
                user.GetRequiredTenantId();

            var workspaceGroupName =
                ConversationGroupName.ForWorkspace(
                    tenantId);

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                workspaceGroupName,
                Context.ConnectionAborted);
        }

        await base.OnConnectedAsync();
    }

    public async Task SubscribeConversation(
        Guid conversationId)
    {
        var user = Context.User
            ?? throw new HubException("Unauthenticated connection.");

        var tenantId =
            user.GetRequiredTenantId();

        var actorType =
            user.GetRequiredActorType();

        switch (actorType)
        {
            case OmniDeskActorTypes.Agent:
                await _conversationAccessService.EnsureCanAccessAsync(
                    tenantId,
                    conversationId,
                    Context.ConnectionAborted);
                break;

            case OmniDeskActorTypes.Customer:
                {
                    var authorizedConversationId =
                        user.GetRequiredConversationId();

                    if (authorizedConversationId != conversationId)
                    {
                        throw new HubException(
                            "You cannot access this conversation.");
                    }

                    break;
                }

            default:
                throw new HubException(
                    "Unsupported actor type.");
        }

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