using OmniDesk.Application.Identity.Models;

namespace OmniDesk.Application.Identity;

public interface IRegistrationService
{
    Task<RegisterTenantResult> RegisterAsync(
        RegisterTenantRequest request,
        CancellationToken cancellationToken);
}