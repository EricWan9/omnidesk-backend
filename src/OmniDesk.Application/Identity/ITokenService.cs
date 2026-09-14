using OmniDesk.Domain.Identity;

namespace OmniDesk.Application.Identity;

public interface ITokenService
{
    string GenerateAccessToken(User user);

    string GenerateCustomerAccessToken(
        Guid tenantId,
        Guid customerId,
        Guid conversationId);

    string GenerateRefreshToken();
}