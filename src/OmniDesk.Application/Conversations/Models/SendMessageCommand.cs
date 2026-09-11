using OmniDesk.Domain.Conversations.Enums;

namespace OmniDesk.Application.Conversations.Models;

public sealed record SendMessageCommand(
    Guid TenantId,
    Guid ConversationId,
    MessageSender MessageSender,
    string Content
);

public sealed record MessageSender
{
    public MessageSenderType Type { get; }
    public Guid? Id { get; }

    private MessageSender(
        MessageSenderType type,
        Guid? id)
    {
        Type = type;
        Id = id;
    }

    public static MessageSender Agent(Guid userId)
        => new(MessageSenderType.Agent, userId);

    public static MessageSender Customer(Guid customerId)
        => new(MessageSenderType.Customer, customerId);

    public static MessageSender Ai(Guid? aiId = null)
        => new(MessageSenderType.Ai, aiId);

    public static MessageSender System()
        => new(MessageSenderType.System, null);
}