using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Entities;

namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        entity.HasIndex(x => x.TokenHash)
            .IsUnique();

        entity.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId);
    }
}
