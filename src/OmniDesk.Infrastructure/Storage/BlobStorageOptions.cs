namespace OmniDesk.Infrastructure.Storage;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string? ConnectionString { get; set; }

    public string? ServiceUri { get; set; }

    public string ContainerName { get; set; } = string.Empty;
}
