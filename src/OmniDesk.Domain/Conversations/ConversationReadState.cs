using OmniDesk.Domain.Identity;

namespace OmniDesk.Domain.Conversations;

public sealed class ConversationReadState
{
    public Guid UserId { get; set; }

    public Guid ConversationId { get; set; }

    public DateTime LastReadAt { get; set; }

}
