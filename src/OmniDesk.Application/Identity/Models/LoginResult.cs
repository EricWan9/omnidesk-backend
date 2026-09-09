namespace OmniDesk.Application.Identity.Models;

public sealed class LoginResult
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}