namespace OmniDesk.Domain.Security;

public static class OmniDeskClaimTypes
{
    public const string TenantId = "tenantId";
    public const string UserId = "userId";

    public const string CustomerId = "customer_id";
    public const string ConversationId = "conversation_id";
    public const string ActorType = "actor_type";
}

public static class OmniDeskActorTypes
{
    public const string Agent = "agent";
    public const string Customer = "customer";
}