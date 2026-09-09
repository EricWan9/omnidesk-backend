using OmniDesk.Application.Identity.Models;

namespace OmniDesk.Application.Identity;

public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}