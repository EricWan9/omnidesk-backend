using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Widgets;

namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class WidgetConfigurationConfiguration : IEntityTypeConfiguration<WidgetConfiguration>
{
    public void Configure(EntityTypeBuilder<WidgetConfiguration> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TenantId)
            .HasMaxLength(100)
            .IsRequired();

        entity.HasIndex(x => x.WidgetKey)
            .IsUnique();

        entity.Property(x => x.CreatedAt).IsRequired();
        entity.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}