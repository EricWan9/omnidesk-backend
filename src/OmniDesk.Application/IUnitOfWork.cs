namespace OmniDesk.Application;

public interface IUnitOfWork
{
    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
