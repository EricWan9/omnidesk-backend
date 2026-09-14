using OmniDesk.Application.Widgets.Models;

namespace OmniDesk.Application.Widgets;

public interface IWidgetConversationService
{
    Task<StartWidgetConversationResult>
        StartConversationAsync(
            StartWidgetConversationCommand command,
            CancellationToken cancellationToken);
}