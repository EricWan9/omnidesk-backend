using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OmniDesk.Infrastructure.Persistence;

public sealed class OmniDeskDbContextFactory
    : IDesignTimeDbContextFactory<OmniDeskDbContext>
{
    public OmniDeskDbContext CreateDbContext(string[] args)
    {
        const string designTimeConnectionString =
            "Server=localhost;" +
            "Database=OmniDesk_DesignTime;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        var options =
            new DbContextOptionsBuilder<OmniDeskDbContext>()
                .UseSqlServer(designTimeConnectionString)
                .Options;

        return new OmniDeskDbContext(options);
    }
}