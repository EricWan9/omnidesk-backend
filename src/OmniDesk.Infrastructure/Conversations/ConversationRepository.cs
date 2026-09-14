using Microsoft.EntityFrameworkCore;
using OmniDesk.Application.Conversations;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Conversations;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly OmniDeskDbContext _dbContext;

    public ConversationRepository(OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConversationDetailResponse?> GetConversationDetailByIdAsync(
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
                c.Customer.Name,
                c.Customer.Email,
                c.Status,
                c.AssignedUserId,
                c.CreatedAt,
                c.UpdatedAt,
                c.RowVersion
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Conversation?> GetConversationByIdAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken
        cancellationToken)
    {
        return _dbContext.Conversations
            .FirstOrDefaultAsync(
            c =>
                c.TenantId == tenantId &&
                c.Id == conversationId,
            cancellationToken);
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
            c.Customer.Name,
            c.Customer.Email,
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

    public void AddConversation(Conversation conversation)
    {
        _dbContext.Conversations.Add(conversation);
    }
}
