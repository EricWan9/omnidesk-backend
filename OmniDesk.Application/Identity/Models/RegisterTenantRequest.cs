namespace OmniDesk.Application.Identity.Models;

public sealed class RegisterTenantRequest
{
    public string OrganizationName { get; set; } = null!;

    public string AdminEmail { get; set; } = null!;

    public string AdminPassword { get; set; } = null!;

    public string AdminDisplayName { get; set; } = null!;
}