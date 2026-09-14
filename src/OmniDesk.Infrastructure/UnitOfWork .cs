using OmniDesk.Application;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OmniDeskDbContext _dbContext;

    public UnitOfWork(OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
