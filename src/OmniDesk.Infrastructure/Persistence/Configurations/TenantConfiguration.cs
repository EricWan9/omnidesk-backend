using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Entities;

namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.HasMany(x => x.Users)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId);
    }
}
