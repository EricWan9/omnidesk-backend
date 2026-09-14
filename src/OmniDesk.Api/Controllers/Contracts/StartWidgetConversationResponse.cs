namespace OmniDesk.Api.Controllers.Contracts;

public sealed record StartWidgetConversationResponse(
    Guid ConversationId,
    string AccessToken);
