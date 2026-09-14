using OmniDesk.Application.Conversations.Exceptions;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Domain.Conversations;

namespace OmniDesk.Application.Conversations;

public sealed class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationNotifier _conversationNotifier;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConversationService(
        IConversationRepository conversationRepository,
        IConversationNotifier conversationNotifier,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _conversationNotifier = conversationNotifier;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConversationDetailResponse?> GetConversationAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken)
    {
        return await _conversationRepository.GetConversationDetailByIdAsync(tenantId, conversationId, cancellationToken);
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

        return await _messageRepository.GetMessagesAsync(
            tenantId,
            conversationId,
            pageSize,
            cancellationToken);
    }

    public async Task<MessageResponse> SendMessageAsync(
        SendMessageCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Content))
        {
            throw new ArgumentException(
                "Message content cannot be empty.",
                nameof(command.Content));
        }

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

        var now = DateTime.UtcNow;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderType = command.MessageSender.Type,
            SenderId = command.MessageSender.Id,
            Content = command.Content,
            CreatedAt = now
        };

        _messageRepository.AddMessage(message);

        conversation.MarkUpdated(now);

        await _unitOfWork.SaveChangesAsync(
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
