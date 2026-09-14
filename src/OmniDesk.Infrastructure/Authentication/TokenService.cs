using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OmniDesk.Application.Identity;
using OmniDesk.Domain.Identity;
using OmniDesk.Domain.Security;

namespace OmniDesk.Infrastructure.Authentication;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(
                OmniDeskClaimTypes.TenantId,
                user.TenantId.ToString()),

            new Claim(
                OmniDeskClaimTypes.UserId,
                user.Id.ToString()),

            new Claim(
                OmniDeskClaimTypes.ActorType,
                OmniDeskActorTypes.Agent),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        return GenerateAccessToken(claims);
    }

    public string GenerateCustomerAccessToken(
        Guid tenantId,
        Guid customerId,
        Guid conversationId)
    {
        var claims = new[]
        {
            new Claim(
                OmniDeskClaimTypes.TenantId,
                tenantId.ToString()),

            new Claim(
                OmniDeskClaimTypes.CustomerId,
                customerId.ToString()),

            new Claim(
                OmniDeskClaimTypes.ConversationId,
                conversationId.ToString()),

            new Claim(
                OmniDeskClaimTypes.ActorType,
                OmniDeskActorTypes.Customer)
        };

        return GenerateAccessToken(claims);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _jwtOptions.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}