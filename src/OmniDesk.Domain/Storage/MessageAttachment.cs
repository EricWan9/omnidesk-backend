namespace OmniDesk.Domain.Storage;

public sealed class MessageAttachment
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string OriginalFileName { get; set; } = null!;

    public string BlobName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public DateTime CreatedAt { get; set; }
}
