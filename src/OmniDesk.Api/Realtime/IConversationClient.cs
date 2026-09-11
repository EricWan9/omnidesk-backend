using OmniDesk.Application.Conversations.Models;

namespace OmniDesk.Api.Realtime;

public interface IConversationClient
{
    Task MessageSent(MessageResponse message);
}