using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Identity;
namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.SenderType)
            .IsRequired();

        entity.Property(x => x.Content)
            .HasMaxLength(10000)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.HasIndex(x => new
        {
            x.ConversationId,
            x.CreatedAt
        });
    }
}
