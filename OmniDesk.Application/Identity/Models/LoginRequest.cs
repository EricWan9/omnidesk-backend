namespace OmniDesk.Application.Identity.Models;

public sealed class LoginRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}