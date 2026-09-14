using Microsoft.EntityFrameworkCore;
using OmniDesk.Application.Widgets;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Customers;

public sealed class WidgetConversationRepository : IWidgetConfigurationRepository
{
    private readonly OmniDeskDbContext _dbContext;
    public WidgetConversationRepository(OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> GetActiveWidgetTenantIdAsync(
        string widgetKey, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.WidgetConfigurations
            .AsNoTracking()
            .Where(w => w.WidgetKey == widgetKey && w.IsActive)
            .Select(w => w.TenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
