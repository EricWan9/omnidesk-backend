using Microsoft.Extensions.Logging;
using OmniDesk.Application.Attachments;
using OmniDesk.Application.Conversations.Exceptions;
using OmniDesk.Application.Conversations.Models;
using OmniDesk.Application.Storage;
using OmniDesk.Domain.Conversations;
using OmniDesk.Domain.Storage;

namespace OmniDesk.Application.Conversations;

public sealed class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationNotifier _conversationNotifier;
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationReadStateRepository _conversationReadStateRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IBlobStorageService _blobStorageService;
    private readonly IMessageAttachmentRepository _messageAttachmentRepository;

    private readonly ILogger<ConversationService> _logger;

    private const long MaxAttachmentSize = 10 * 1024 * 1024;
    private const int MaxAttachmentCount = 5;
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf",
        "text/plain"
    ];

    public ConversationService(
        IConversationRepository conversationRepository,
        IConversationNotifier conversationNotifier,
        IMessageRepository messageRepository,
        IConversationReadStateRepository conversationReadStateRepository,
        IUnitOfWork unitOfWork,
        IBlobStorageService blobStorageService,
        IMessageAttachmentRepository messageAttachmentRepository,
        ILogger<ConversationService> logger)
    {
        _conversationRepository = conversationRepository;
        _conversationNotifier = conversationNotifier;
        _messageRepository = messageRepository;
        _conversationReadStateRepository = conversationReadStateRepository;
        _unitOfWork = unitOfWork;
        _blobStorageService = blobStorageService;
        _messageAttachmentRepository = messageAttachmentRepository;
        _logger = logger;
    }

    public async Task<ConversationDetailResponse?> GetConversationAsync(
        Guid tenantId,
        Guid conversationId, 
        CancellationToken cancellationToken)
    {
        return await _conversationRepository.GetConversationDetailByIdAsync(
            tenantId, 
            conversationId, 
            cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationListItemResponse>> GetConversationsAsync(
        Guid tenantId, 
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _conversationRepository.GetConversationsAsync(
            tenantId,
            userId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid tenantId, 
        Guid conversationId, 
        int pageSize, 
        CancellationToken cancellationToken)
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
    SendMessageCommand command,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Content) &&
            command.Attachments.Count == 0)
        {
            throw new ArgumentException(
                "A message must contain text or at least one attachment.");
        }

        ValidateAttachments(command.Attachments);

        var conversation =
            await _conversationRepository.GetConversationByIdAsync(
                command.TenantId,
                command.ConversationId,
                cancellationToken);

        if (conversation is null)
        {
            throw new ConversationNotFoundException(
                command.ConversationId);
        }

        if (conversation.Status == ConversationStatus.Closed)
        {
            throw new ConversationClosedException(command.ConversationId);
        }

        var now = DateTime.UtcNow;
        var messageId = Guid.NewGuid();

        var message = new Message
        {
            Id = messageId,
            ConversationId = command.ConversationId,
            SenderType = command.MessageSender.Type,
            SenderId = command.MessageSender.Id,
            Content = string.IsNullOrWhiteSpace(command.Content)
                ? null
                : command.Content.Trim(),
            CreatedAt = now
        };

        var uploadedBlobNames = new List<string>();

        var messageAttachments =
            new List<MessageAttachment>();

        try
        {
            foreach (var upload in command.Attachments)
            {
                var attachmentId = Guid.NewGuid();

                var blobName =
                    BuildBlobName(
                        command.TenantId,
                        command.ConversationId,
                        attachmentId,
                        upload.FileName);

                await _blobStorageService.UploadAsync(
                    upload.Content,
                    blobName,
                    upload.ContentType,
                    cancellationToken);

                uploadedBlobNames.Add(blobName);

                var attachment =
                    new MessageAttachment
                    {
                        Id = attachmentId,
                        MessageId = messageId,
                        OriginalFileName =
                            Path.GetFileName(upload.FileName),
                        BlobName = blobName,
                        ContentType = upload.ContentType,
                        Size = upload.Length,
                        CreatedAt = now
                    };

                messageAttachments.Add(attachment);
            }

            _messageRepository.AddMessage(message);

            foreach (var attachment in messageAttachments)
            {
                _messageAttachmentRepository.Add(
                    attachment);
            }

            conversation.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            foreach (var blobName in uploadedBlobNames)
            {
                try
                {
                    await _blobStorageService.DeleteAsync(
                        blobName,
                        CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(
                        cleanupException,
                        "Failed to clean up orphan blob {BlobName}.",
                        blobName);
                    // Best-effort cleanup.
                    // run an orphan blob cleanup job.
                }
            }

            throw;
        }

        var attachmentResponses =
            messageAttachments
                .Select(a =>
                    new MessageAttachmentResponse(
                        a.Id,
                        a.OriginalFileName,
                        a.ContentType,
                        a.Size))
                .ToList();

        var response =
            new MessageResponse(
                message.Id,
                message.ConversationId,
                new MessageSenderResponse(
                    message.SenderType,
                    message.SenderId),
                message.Content,
                message.CreatedAt,
                attachmentResponses);

        await _conversationNotifier.MessageSentAsync(
            command.TenantId,
            command.ConversationId,
            response,
            cancellationToken);

        return response;
    }

    public async Task MarkAsReadAsync(
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetConversationByIdAsync(
            tenantId,
            conversationId,
            cancellationToken);
        if(conversation == null)
        {
            throw new ConversationNotFoundException(conversationId);
        }

        var readState =
            await _conversationReadStateRepository.GetAsync(
                userId,
                conversationId,
                cancellationToken);

        var now = DateTime.UtcNow;

        if (readState is null)
        {
            readState =
                new ConversationReadState
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    LastReadAt = now
                };

            _conversationReadStateRepository.Add(
                readState);
        }
        else
        {
            readState.LastReadAt = now;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    private static string BuildBlobName(
        Guid tenantId,
        Guid conversationId,
        Guid attachmentId,
        string originalFileName)
    {
        var extension =
            Path.GetExtension(originalFileName);

        return
            $"{tenantId:N}/{conversationId:N}/{attachmentId:N}{extension}";
    }

    private static void ValidateAttachments(
        IReadOnlyList<AttachmentUpload> attachments)
    {
        if (attachments.Count > MaxAttachmentCount)
        {
            throw new ArgumentException(
                $"A message can contain at most {MaxAttachmentCount} attachments.");
        }

        foreach (var attachment in attachments)
        {
            if (attachment.Length <= 0)
            {
                throw new ArgumentException(
                    "Attachment cannot be empty.");
            }

            if (attachment.Length > MaxAttachmentSize)
            {
                throw new ArgumentException(
                    $"Attachment cannot exceed {MaxAttachmentSize / 1024 / 1024} MB.");
            }

            if (!AllowedContentTypes.Contains(
                    attachment.ContentType))
            {
                throw new ArgumentException(
                    $"Unsupported attachment type: {attachment.ContentType}");
            }
        }
    }
}
