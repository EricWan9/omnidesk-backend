using Microsoft.EntityFrameworkCore;
using OmniDesk.Application.Conversations;
using OmniDesk.Domain.Conversations;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Conversations;

public sealed class ConversationReadStateRepository : IConversationReadStateRepository
{
    private readonly OmniDeskDbContext _dbContext;

    public ConversationReadStateRepository(OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(ConversationReadState readState)
    {
        _dbContext.ConversationReadStates.Add(readState);
    }

    public async Task<ConversationReadState?> GetAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken)
    {
        return await _dbContext.ConversationReadStates
            .SingleOrDefaultAsync( x =>
                x.UserId == userId
                && x.ConversationId == conversationId,
            cancellationToken);
    }
}
