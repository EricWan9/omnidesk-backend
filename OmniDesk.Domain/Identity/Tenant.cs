using OmniDesk.Domain.Identity;

namespace OmniDesk.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}