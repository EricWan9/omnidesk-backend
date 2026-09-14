using Microsoft.Extensions.Options;
using OmniDesk.Domain.Identity;
using OmniDesk.Domain.Security;
using OmniDesk.Infrastructure.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace OmniDesk.Application.Tests.Authentication;

public class TokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_ShouldContainAgentClaims()
    {
        var options = CreateJwtOptions();
        var service = new TokenService(options);
        var user = CreateUser();

        var token = service.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(
            user.Id.ToString(),
            jwtToken.Claims
                .First(c => c.Type == OmniDeskClaimTypes.UserId)
                .Value);

        Assert.Equal(
            user.TenantId.ToString(),
            jwtToken.Claims
                .First(c => c.Type == OmniDeskClaimTypes.TenantId)
                .Value);

        Assert.Equal(
            OmniDeskActorTypes.Agent,
            jwtToken.Claims
                .First(c => c.Type == OmniDeskClaimTypes.ActorType)
                .Value);
    }

    [Fact]
    public void GenerateCustomerAccessToken_ShouldContainCustomerClaims()
    {
        var options = CreateJwtOptions();
        var service = new TokenService(options);

        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var token =
            service.GenerateCustomerAccessToken(
                tenantId,
                customerId,
                conversationId);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(
            tenantId.ToString(),
            jwtToken.Claims
                .First(c => c.Type == OmniDeskClaimTypes.TenantId)
                .Value);

        Assert.Equal(
            customerId.ToString(),
            jwtToken.Claims
                .First(c => c.Type == OmniDeskClaimTypes.CustomerId)
                .Value);

        Assert.Equal(
            conversationId.ToString(),
            jwtToken.Claims
                .First(c => c.Type == OmniDeskClaimTypes.ConversationId)
                .Value);

        Assert.Equal(
            OmniDeskActorTypes.Customer,
            jwtToken.Claims
                .First(c => c.Type == OmniDeskClaimTypes.ActorType)
                .Value);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainRoleClaim()
    {
        // Arrange
        var options = CreateJwtOptions();

        var service = new TokenService(options);

        var user = CreateUser();

        // Act
        var tokenString =
            service.GenerateAccessToken(user);

        var token =
            new JwtSecurityTokenHandler()
                .ReadJwtToken(tokenString);

        // Assert
        Assert.Contains(
            token.Claims,
            claim =>
                claim.Type == ClaimTypes.Role &&
                claim.Value == user.Role);
    }

    private static IOptions<JwtOptions> CreateJwtOptions()
    {
        return Microsoft.Extensions.Options.Options.Create(
            new JwtOptions
            {
                Issuer = "OmniDesk.Identity.Tests",
                Audience = "OmniDesk.Tests",
                Key =
                    "THIS_IS_A_TEST_KEY_AND_IT_MUST_BE_LONG_ENOUGH_123456789",
                AccessTokenMinutes = 15,
                RefreshTokenDays = 30
            });
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "agent@omnidesk.test",
            Role = "Agent"
        };
    }
}
