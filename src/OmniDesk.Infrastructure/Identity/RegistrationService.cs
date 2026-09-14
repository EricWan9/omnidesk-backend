using OmniDesk.Application.Identity;
using OmniDesk.Application.Identity.Models;
using OmniDesk.Domain.Entities;
using OmniDesk.Domain.Identity;
using OmniDesk.Domain.Widgets;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Identity;

public sealed class RegistrationService : IRegistrationService
{
    private readonly OmniDeskDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public RegistrationService(
        OmniDeskDbContext dbContext,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<RegisterTenantResult> RegisterAsync(
        RegisterTenantRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.AdminEmail
            .Trim()
            .ToLowerInvariant();

        var now = DateTime.UtcNow;

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName.Trim(),
            IsActive = true,
            CreatedAt = now
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = normalizedEmail,
            DisplayName = request.AdminDisplayName.Trim(),
            Role = "Admin",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var widgetConfiguration = new WidgetConfiguration
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            WidgetKey = $"wdg_{Guid.NewGuid():N}",
            IsActive = true,
            CreatedAt = now
        };

        user.PasswordHash =
            _passwordService.HashPassword(user, request.AdminPassword);

        var refreshTokenValue =
            _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenValue,
            CreatedAt = now,
            ExpiresAt = now.AddDays(30)
        };

        var accessToken =
            _tokenService.GenerateAccessToken(user);

        _dbContext.Tenants.Add(tenant);
        _dbContext.Users.Add(user);
        _dbContext.RefreshTokens.Add(refreshToken);
        _dbContext.WidgetConfigurations.Add(widgetConfiguration);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterTenantResult
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue
        };
    }
}
