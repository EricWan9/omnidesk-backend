using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Entities;

namespace OmniDesk.Domain.Customers;

public class Customer
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
