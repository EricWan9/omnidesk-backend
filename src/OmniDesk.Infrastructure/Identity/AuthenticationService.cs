using Microsoft.EntityFrameworkCore;
using OmniDesk.Application.Identity;
using OmniDesk.Application.Identity.Models;
using OmniDesk.Domain.Entities;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Identity;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly OmniDeskDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthenticationService(
        OmniDeskDbContext dbContext,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(x => x.Tenant)
            .SingleOrDefaultAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (!user.IsActive || !user.Tenant.IsActive)
        {
            throw new UnauthorizedAccessException();
        }

        var passwordValid = _passwordService.VerifyPassword(
            user,
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException();
        }

        var accessToken =
            _tokenService.GenerateAccessToken(user);

        var refreshTokenValue =
            _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,

            TokenHash = refreshTokenValue,

            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RevokedAt = null
        };

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResult
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue
        };
    }
}
