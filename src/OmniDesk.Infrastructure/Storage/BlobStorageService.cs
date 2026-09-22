using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using OmniDesk.Application.Storage;

namespace OmniDesk.Infrastructure.Storage;

public sealed class BlobStorageService
    : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private readonly bool _ensureContainerExists;

    public BlobStorageService(
        IOptions<BlobStorageOptions> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(
            settings.ContainerName))
        {
            throw new InvalidOperationException(
                "BlobStorage:ContainerName is missing.");
        }

        if (!string.IsNullOrWhiteSpace(
            settings.ConnectionString))
        {
            _container = new BlobContainerClient(
                settings.ConnectionString,
                settings.ContainerName);

            _ensureContainerExists = true;

            return;
        }

        if (!string.IsNullOrWhiteSpace(
            settings.ServiceUri))
        {
            var blobServiceClient =
                new BlobServiceClient(
                    new Uri(settings.ServiceUri),
                    new ManagedIdentityCredential(
                        ManagedIdentityId.SystemAssigned));

            _container =
                blobServiceClient
                    .GetBlobContainerClient(
                        settings.ContainerName);

            _ensureContainerExists = false;

            return;
        }

        throw new InvalidOperationException(
            "Blob Storage configuration is missing. " +
            "Configure either BlobStorage:ConnectionString " +
            "or BlobStorage:ServiceUri.");
    }


    public async Task<string> UploadAsync(
        Stream stream,
        string blobName,
        string contentType,
        CancellationToken cancellationToken)
    {
        if (_ensureContainerExists)
        {
            await _container.CreateIfNotExistsAsync(
                cancellationToken:
                    cancellationToken);
        }

        var blob =
            _container.GetBlobClient(
                blobName);

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
            _container.GetBlobClient(
                blobName);

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
            _container.GetBlobClient(
                blobName);

        await blob.DeleteIfExistsAsync(
            cancellationToken:
                cancellationToken);
    }
}