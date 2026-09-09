using OmniDesk.Domain.Identity;

namespace OmniDesk.Application.Identity;

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string password, string passwordHash);
}