using System.Security.Claims;

namespace OmniDesk.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredTenantId(
        this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(
            OmniDeskClaimTypes.TenantId);

        if (!Guid.TryParse(value, out var tenantId))
        {
            throw new UnauthorizedAccessException(
                "Tenant claim is missing or invalid.");
        }

        return tenantId;
    }

    public static Guid GetRequiredUserId(
        this ClaimsPrincipal user)
    {
        var value =
            user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(OmniDeskClaimTypes.UserId);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User claim is missing or invalid.");
        }

        return userId;
    }
}