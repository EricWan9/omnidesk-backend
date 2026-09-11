using OmniDesk.Application.Conversations.Exceptions;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations.Entities;
using OmniDesk.Domain.Conversations.Enums;

namespace OmniDesk.Application.Conversations;

public sealed class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationNotifier _conversationNotifier;

    public ConversationService(
        IConversationRepository conversationRepository,
        IConversationNotifier conversationNotifier)
    {
        _conversationRepository = conversationRepository;
        _conversationNotifier = conversationNotifier;
    }

    public async Task<ConversationDetailResponse?> GetConversationAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken)
    {
        return await _conversationRepository.GetConversationByIdAsync(tenantId, conversationId, cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationListItemResponse>> GetConversationsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _conversationRepository.GetConversationsAsync(tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid tenantId, Guid conversationId, int pageSize, CancellationToken cancellationToken)
    {
        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        return await _conversationRepository.GetMessagesAsync(
            tenantId,
            conversationId,
            pageSize,
            cancellationToken);
    }

    public async Task<MessageResponse> SendMessageAsync(
        SendMessageCommand command, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetConversationByIdAsync(
            command.TenantId, command.ConversationId, cancellationToken);

        if(conversation == null)
        {
            throw new ConversationNotFoundException(command.ConversationId);
        }

        if (conversation.Status == ConversationStatus.Closed)
        {
            throw new ConversationClosedException(command.ConversationId);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = command.ConversationId,
            SenderType = command.MessageSender.Type,
            SenderId = command.MessageSender.Id,
            Content = command.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _conversationRepository.AddMessageAsync(
            message,
            cancellationToken);

        await _conversationRepository.SaveChangesAsync(
            cancellationToken);

        var response = new MessageResponse(
            message.Id,
            command.ConversationId,
            new MessageSenderResponse(
                command.MessageSender.Type, 
                command.MessageSender.Id),
            message.Content,
            message.CreatedAt
        );

        await _conversationNotifier.MessageSentAsync(
            command.TenantId,
            command.ConversationId,
            response,
            cancellationToken);

        return response;
    }
}
