using OmniDesk.Domain.Entities;

namespace OmniDesk.Domain.Widgets;

public class WidgetConfiguration
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string WidgetKey { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}