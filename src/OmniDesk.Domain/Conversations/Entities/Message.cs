using OmniDesk.Domain.Conversations.Enums;

namespace OmniDesk.Domain.Conversations.Entities;

public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public MessageSenderType SenderType { get; set; }

    public Guid? SenderId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}