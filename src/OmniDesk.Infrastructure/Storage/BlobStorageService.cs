using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using OmniDesk.Application.Storage;

namespace OmniDesk.Infrastructure.Storage;

public sealed class BlobStorageService
    : IBlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(
        IOptions<BlobStorageOptions> options)
    {
        var settings = options.Value;

        _container = new BlobContainerClient(
            settings.ConnectionString,
            settings.ContainerName);
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string blobName,
        string contentType,
        CancellationToken cancellationToken)
    {
        await _container.CreateIfNotExistsAsync(
            cancellationToken:
                cancellationToken);

        var blob =
            _container.GetBlobClient(blobName);

        await blob.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders =
                    new BlobHttpHeaders
                    {
                        ContentType =
                            contentType
                    }
            },
            cancellationToken);

        return blobName;
    }

    public async Task<Stream> OpenReadAsync(
        string blobName,
        CancellationToken cancellationToken)
        {
            var blob =
                _container.GetBlobClient(blobName);

            var response =
                await blob.DownloadStreamingAsync(
                    cancellationToken:
                        cancellationToken);

            return response.Value.Content;
        }

    public async Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken)
    {
        var blob =
            _container.GetBlobClient(blobName);

        await blob.DeleteIfExistsAsync(
            cancellationToken:
                cancellationToken);
    }
}
