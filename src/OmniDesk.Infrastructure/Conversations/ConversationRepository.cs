using Microsoft.EntityFrameworkCore;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations.Entities;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Conversations;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly OmniDeskDbContext _dbContext;

    public ConversationRepository(OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddMessageAsync(Message message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<ConversationDetailResponse?> GetConversationByIdAsync(
        Guid tenantId, 
        Guid conversationId, 
        CancellationToken 
        cancellationToken)
    {
        return await _dbContext.Conversations
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Id == conversationId)
            .Select(c => new ConversationDetailResponse(
                c.Id,
                c.CustomerName,
                c.CustomerEmail,
                c.Status,
                c.AssignedUserId,
                c.CreatedAt,
                c.UpdatedAt,
                c.RowVersion
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationListItemResponse>> GetConversationsAsync(
        Guid tenantId, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.Conversations
        .AsNoTracking()
        .Where(c => c.TenantId == tenantId)
        .Select(c => new ConversationListItemResponse(
            c.Id,
            c.CustomerName,
            c.CustomerEmail,
            c.Status,
            c.AssignedUserId,

            c.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.Content)
                .FirstOrDefault(),

            c.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => (DateTime?)m.CreatedAt)
                .FirstOrDefault(),

            c.UpdatedAt,
            c.RowVersion
        ))
        .ToListAsync(cancellationToken);
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

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
