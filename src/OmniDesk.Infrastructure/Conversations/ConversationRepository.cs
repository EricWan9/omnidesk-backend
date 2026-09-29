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

    public async Task<ConversationListItemResponse?>
    GetConversationDetailByIdAsync(
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var lastReadAt =
            await _dbContext.ConversationReadStates
                .AsNoTracking()
                .Where(r =>
                    r.UserId == userId &&
                    r.ConversationId == conversationId)
                .Select(r => (DateTime?)r.LastReadAt)
                .FirstOrDefaultAsync(cancellationToken);

        return await _dbContext.Conversations
            .AsNoTracking()
            .Where(c =>
                c.TenantId == tenantId &&
                c.Id == conversationId)
            .Select(c =>
                new ConversationListItemResponse(
                    c.Id,

                    c.Customer == null
                        ? null
                        : c.Customer.Name,

                    c.Customer == null
                        ? null
                        : c.Customer.Email,

                    c.Status,

                    c.AssignedUserId,
                    _dbContext.Users
                        .Where(u => u.Id == c.AssignedUserId)
                        .Select(u => u.DisplayName)
                        .FirstOrDefault(),

                    c.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .ThenByDescending(m => m.Id)
                        .Select(m => m.Content)
                        .FirstOrDefault(),

                    c.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .ThenByDescending(m => m.Id)
                        .Select(m => (DateTime?)m.CreatedAt)
                        .FirstOrDefault(),

                    c.UpdatedAt,

                    c.Messages
                        .Where(m =>
                            m.SenderType ==
                                MessageSenderType.Customer
                            &&
                            (
                                !lastReadAt.HasValue ||
                                m.CreatedAt >
                                    lastReadAt.Value
                            ))
                        .Take(100)
                        .Count(),

                    c.RowVersion
                ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Conversation?> GetConversationByIdAsync(
        Guid tenantId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Conversations
            .FirstOrDefaultAsync(
            c =>
                c.TenantId == tenantId &&
                c.Id == conversationId,
            cancellationToken);
    }

    public async Task<GetConversationsResult> GetConversationsAsync(
        Guid tenantId,
        Guid userId,
        int page,
        int pageSize,
        ConversationStatusFilter statusFilter,
        ConversationAssignmentFilter assignmentFilter,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Conversations
        .AsNoTracking()
        .Where(c => c.TenantId == tenantId);

        // Apply status filter
        query = statusFilter switch
        {
            ConversationStatusFilter.Open => query.Where(c => c.Status == ConversationStatus.Open),
            ConversationStatusFilter.Closed => query.Where(c => c.Status == ConversationStatus.Closed),
            _ => query
        };

        // Apply assignment filter
        query = assignmentFilter switch
        {
            ConversationAssignmentFilter.Mine => query.Where(c => c.AssignedUserId == userId),
            ConversationAssignmentFilter.Unassigned => query.Where(c => c.AssignedUserId == null),
            _ => query
        };

        // Apply search filter (customer name / email)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmed = search.Trim();
            query = query.Where(c =>
                (c.Customer != null && (
                    EF.Functions.Like(c.Customer.Name, $"%{trimmed}%") ||
                    EF.Functions.Like(c.Customer.Email, $"%{trimmed}%")
                ))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var conversations = await query
        .OrderByDescending(c => c.UpdatedAt)
        .ThenByDescending(c => c.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new ConversationListItemResponse(
            c.Id,
            c.Customer.Name,
            c.Customer.Email,
            c.Status,
            c.AssignedUserId,
            // Project assignee display name if present
            _dbContext.Users
                .Where(u => u.Id == c.AssignedUserId)
                .Select(u => u.DisplayName)
                .FirstOrDefault(),

            c.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.Content)
                .FirstOrDefault(),

            c.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => (DateTime?)m.CreatedAt)
                .FirstOrDefault(),

            c.UpdatedAt,

            c.Messages.Count(m =>
                m.SenderType == MessageSenderType.Customer
                && (
                    !_dbContext.ConversationReadStates.Any(r =>
                        r.UserId == userId
                        && r.ConversationId == c.Id)
                    ||
                    m.CreatedAt >
                    _dbContext.ConversationReadStates
                        .Where(r =>
                            r.UserId == userId
                            && r.ConversationId == c.Id)
                        .Select(r => r.LastReadAt)
                        .FirstOrDefault()
                )
            ),

            c.RowVersion
        ))
        .ToListAsync(cancellationToken);

        return new GetConversationsResult(
            conversations,
            page, 
            pageSize, 
            totalCount, 
            (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public void AddConversation(Conversation conversation)
    {
        _dbContext.Conversations.Add(conversation);
    }
}
