namespace OmniDesk.Infrastructure.Storage;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string ConnectionString { get; init; } = null!;

    public string ContainerName { get; init; } = null!;
}
