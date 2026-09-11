namespace OmniDesk.Application.Conversations;

public interface IConversationAccessService
{
    Task EnsureCanAccessAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken);
}