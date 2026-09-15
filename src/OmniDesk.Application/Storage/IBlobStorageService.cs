namespace OmniDesk.Application.Storage;

public interface IBlobStorageService
{
    Task<string> UploadAsync(
        Stream stream,
        string blobName,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        string blobName,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken);
}