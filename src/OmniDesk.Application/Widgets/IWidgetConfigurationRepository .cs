namespace OmniDesk.Application.Widgets;

public interface IWidgetConfigurationRepository
{
    Task<Guid?> GetActiveWidgetTenantIdAsync(
       string widgetKey,
       CancellationToken cancellationToken);
}
