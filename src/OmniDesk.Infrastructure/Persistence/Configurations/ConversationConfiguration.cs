using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Entities;
using OmniDesk.Domain.Identity;

namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Status)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.Property(x => x.UpdatedAt)
            .IsRequired();

        entity.Property(x => x.RowVersion)
            .IsRowVersion();

        entity.HasIndex(x => new
        {
            x.TenantId,
            x.CustomerId
        });

        entity.HasIndex(x => new
        {
            x.TenantId,
            x.UpdatedAt
        });

        entity.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
