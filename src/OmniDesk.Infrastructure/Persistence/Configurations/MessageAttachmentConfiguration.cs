using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Storage;

namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class MessageAttachmentConfiguration
    : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(
        EntityTypeBuilder<MessageAttachment> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        entity.Property(x => x.BlobName)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        entity.HasIndex(x => x.MessageId);

        entity.HasOne<Message>()
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}