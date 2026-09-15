using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Identity;

namespace OmniDesk.Infrastructure.Persistence.Configurations;

public sealed class ConversationReadStateResaultConfiguration
    : IEntityTypeConfiguration<ConversationReadState>
{
    public void Configure(EntityTypeBuilder<ConversationReadState> entity)
    {
        entity.HasKey(x => new
        {
            x.UserId,
            x.ConversationId
        });

        entity.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
