using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Api.Realtime.Conversations;

public interface IConversationClient
{
    Task MessageSent(MessageResponse message);
}