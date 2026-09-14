using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Identity;

namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        entity.Property(x => x.PasswordHash)
            .IsRequired();

        entity.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.Role)
            .HasMaxLength(50)
            .IsRequired();

        entity.HasIndex(x => new
        {
            x.TenantId,
            x.Email
        })
        .IsUnique();
    }
}
