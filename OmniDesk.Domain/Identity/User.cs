using OmniDesk.Domain.Entities;

namespace OmniDesk.Domain.Identity;

public class User
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}