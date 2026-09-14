using Microsoft.EntityFrameworkCore;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Conversations;

public sealed class MessageRepository : IMessageRepository
{
    private readonly OmniDeskDbContext _dbContext;

    public MessageRepository(OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddMessage(Message message)
    {
        _dbContext.Messages.Add(message);
    }

    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid tenantId,
        Guid conversationId,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.Conversation.TenantId == tenantId && m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(pageSize)
            .Select(m => new MessageResponse(
                m.Id,
                m.ConversationId,
                new MessageSenderResponse(m.SenderType, m.SenderId),
                m.Content,
                m.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
