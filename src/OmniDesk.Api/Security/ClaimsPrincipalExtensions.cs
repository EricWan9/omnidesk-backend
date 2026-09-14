using OmniDesk.Domain.Security;
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

    public static Guid GetRequiredCustomerId(
        this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(
            OmniDeskClaimTypes.CustomerId);

        if (!Guid.TryParse(value, out var customerId))
        {
            throw new UnauthorizedAccessException(
                "Customer ID claim is missing or invalid.");
        }

        return customerId;
    }

    public static Guid GetRequiredConversationId(
        this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(
            OmniDeskClaimTypes.ConversationId);

        if (!Guid.TryParse(value, out var conversationId))
        {
            throw new UnauthorizedAccessException(
                "Conversation ID claim is missing or invalid.");
        }

        return conversationId;
    }

    public static string GetRequiredActorType(
        this ClaimsPrincipal user)
    {
        return user.FindFirstValue(
            OmniDeskClaimTypes.ActorType)
            ?? throw new UnauthorizedAccessException(
                "Actor type claim is missing.");
    }

    public static void EnsureCustomerActor(
        this ClaimsPrincipal user)
    {
        var actorType = user.GetRequiredActorType();

        if (!string.Equals(
                actorType,
                OmniDeskActorTypes.Customer,
                StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException(
                "The current identity is not a customer.");
        }
    }
}