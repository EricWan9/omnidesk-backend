using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Customers;
using OmniDesk.Domain.Entities;
using OmniDesk.Domain.Identity;

namespace OmniDesk.Infrastructure.Persistence;

public class OmniDeskDbContext : DbContext
{
    public OmniDeskDbContext(
        DbContextOptions<OmniDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OmniDeskDbContext).Assembly);
    }
}