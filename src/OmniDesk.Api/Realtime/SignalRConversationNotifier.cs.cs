using Microsoft.AspNetCore.SignalR;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Api.Realtime;

public sealed class SignalRConversationNotifier : IConversationNotifier
{
    private readonly IHubContext<ConversationHub, IConversationClient> _hubContext;

    public SignalRConversationNotifier(
        IHubContext<ConversationHub, IConversationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task MessageSentAsync(
        Guid tenantId,
        Guid conversationId,
        MessageResponse message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var groupName =
            ConversationGroupName.ForConversation(
                tenantId,
                conversationId);

        return _hubContext.Clients
            .Group(groupName)
            .MessageSent(message);
    }
}
