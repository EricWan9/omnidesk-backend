using Microsoft.AspNetCore.SignalR;
using OmniDesk.Api.Controllers.Contracts;
using OmniDesk.Domain.Conversations;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Api.Realtime.Conversations;

public sealed class SignalRConversationNotifier : IConversationNotifier
{
    private readonly IHubContext<ConversationHub, IConversationClient> _hubContext;

    public SignalRConversationNotifier(
        IHubContext<ConversationHub, IConversationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task MessageSentAsync(
        Guid tenantId,
        Guid conversationId,
        MessageResponse message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _hubContext.Clients
            .Group(
                ConversationGroupName
                    .ForConversation(
                        tenantId,
                        conversationId))
            .MessageSent(message);

        // Notify workspace that conversation was updated (includes last message info)
        await _hubContext.Clients
            .Group(
                ConversationGroupName
                    .ForWorkspace(tenantId))
            .ConversationUpdated(
                new ConversationUpdatedMessage(
                    conversationId,
                    message.MessageSender.Type,
                    message.Content,
                    message.CreatedAt));
    }

    public async Task ConversationUpdatedAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _hubContext.Clients
            .Group(
                ConversationGroupName
                    .ForWorkspace(tenantId))
            .ConversationUpdated(
                new ConversationUpdatedMessage(
                    conversationId,
                    MessageSenderType.System,
                    null,
                    DateTime.UtcNow));
    }
}
