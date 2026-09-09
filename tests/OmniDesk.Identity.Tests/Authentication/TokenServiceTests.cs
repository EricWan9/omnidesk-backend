using Microsoft.Extensions.Options;
using OmniDesk.Domain.Identity;
using OmniDesk.Infrastructure.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace OmniDesk.Identity.Tests.Authentication;

public class TokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_ShouldContainUserAndTenantClaims()
    {
        var options = CreateJwtOptions();

        var service = new TokenService(options);

        var user = CreateUser();

        var token = service.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.NotNull(jwtToken);
        Assert.Equal(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == "userId").Value);
        Assert.Equal(user.TenantId.ToString(), jwtToken.Claims.First(c => c.Type == "tenantId").Value);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == "email").Value);
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
