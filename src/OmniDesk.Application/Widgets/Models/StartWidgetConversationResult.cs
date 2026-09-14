namespace OmniDesk.Application.Widgets.Models;

public sealed record StartWidgetConversationResult(
    Guid CustomerId,
    Guid ConversationId,
    string AccessToken);