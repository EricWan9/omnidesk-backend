using Microsoft.EntityFrameworkCore;
using OmniDesk.Application.Attachments;
using OmniDesk.Application.Conversations;
using OmniDesk.Domain.Storage;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Conversations;

public sealed class MessageAttachmentRepository
    : IMessageAttachmentRepository
{
    private readonly OmniDeskDbContext _dbContext;

    public MessageAttachmentRepository(
        OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(MessageAttachment attachment)
    {
        _dbContext.MessageAttachments.Add(attachment);
    }

    public async Task<AttachmentDownloadInfo?> GetDownloadInfoAsync(
         Guid attachmentId,
         CancellationToken cancellationToken)
    {
        return await _dbContext.MessageAttachments
            .AsNoTracking()
            .Where(a => a.Id == attachmentId)
            .Join(
                _dbContext.Messages,
                attachment => attachment.MessageId,
                message => message.Id,
                (attachment, message) => new
                {
                    Attachment = attachment,
                    Message = message
                })
            .Join(
                _dbContext.Conversations,
                x => x.Message.ConversationId,
                conversation => conversation.Id,
                (x, conversation) =>
                    new AttachmentDownloadInfo(
                        x.Attachment.Id,
                        conversation.Id,
                        conversation.TenantId,
                        x.Attachment.BlobName,
                        x.Attachment.OriginalFileName,
                        x.Attachment.ContentType))
            .FirstOrDefaultAsync(cancellationToken);
    }
}