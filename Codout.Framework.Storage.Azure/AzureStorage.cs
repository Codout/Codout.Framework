using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Codout.Framework.Storage.Configuration;
using Codout.Framework.Storage.Exceptions;
using HeyRed.Mime;

namespace Codout.Framework.Storage.Azure;

/// <summary>
/// Azure Blob Storage implementation of IStorage
/// </summary>
public class AzureStorage : IStorage
{
    private readonly Lazy<BlobServiceClient> _clientLazy;
    private readonly AzureStorageOptions _options;

    public AzureStorage(string connectionString)
        : this(new AzureStorageOptions { ConnectionString = connectionString })
    {
    }

    public AzureStorage(AzureStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required.", nameof(options));

        _options = options;
        _clientLazy = new Lazy<BlobServiceClient>(() => new BlobServiceClient(_options.ConnectionString));
    }

    private BlobServiceClient Client => _clientLazy.Value;

    #region Upload Operations

    public Task<Uri> UploadAsync(Stream file, string container, string fileName, CancellationToken cancellationToken = default)
    {
        return UploadAsync(file, container, fileName, (IDictionary<string, string>?)null, cancellationToken);
    }

    public async Task<Uri> UploadAsync(Stream file, string container, string fileName, IDictionary<string, string>? metadata, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ValidateParameters(container, fileName);

        try
        {
            file.Seek(0, SeekOrigin.Begin);

            var blobContainer = await GetOrCreateContainerAsync(container, cancellationToken);
            var blob = blobContainer.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = MimeTypesMap.GetMimeType(fileName)
            };

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Metadata = metadata
            };

            await blob.UploadAsync(file, uploadOptions, cancellationToken);

            return blob.Uri;
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to upload file '{fileName}' to container '{container}'.", container, fileName, ex);
        }
    }

    public async Task<Uri> UploadAsync(Stream file, string container, string fileName, IProgress<long>? progress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ValidateParameters(container, fileName);

        try
        {
            file.Seek(0, SeekOrigin.Begin);

            var blobContainer = await GetOrCreateContainerAsync(container, cancellationToken);
            var blob = blobContainer.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = MimeTypesMap.GetMimeType(fileName)
            };

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                ProgressHandler = progress != null ? new Progress<long>(progress.Report) : null
            };

            await blob.UploadAsync(file, uploadOptions, cancellationToken);

            return blob.Uri;
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to upload file '{fileName}' to container '{container}'.", container, fileName, ex);
        }
    }

    #endregion

    #region Download Operations

    public async Task<Stream> DownloadAsync(string container, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateParameters(container, fileName);

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
            var blob = blobContainer.GetBlobClient(fileName);

            if (!await blob.ExistsAsync(cancellationToken))
                throw new StorageNotFoundException(container, fileName);

            var stream = new MemoryStream();
            await blob.DownloadToAsync(stream, cancellationToken);
            stream.Position = 0;
            return stream;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new StorageNotFoundException(container, fileName, ex);
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to download file '{fileName}' from container '{container}'.", container, fileName, ex);
        }
    }

    public async Task<Stream> GetStreamAsync(string container, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateParameters(container, fileName);

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
            var blob = blobContainer.GetBlobClient(fileName);

            if (!await blob.ExistsAsync(cancellationToken))
                throw new StorageNotFoundException(container, fileName);

            return await blob.OpenReadAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new StorageNotFoundException(container, fileName, ex);
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to get stream for file '{fileName}' from container '{container}'.", container, fileName, ex);
        }
    }

    #endregion

    #region Delete Operations

    public async Task DeleteAsync(string container, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateParameters(container, fileName);

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
            var blob = blobContainer.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to delete file '{fileName}' from container '{container}'.", container, fileName, ex);
        }
    }

    public async Task DeleteManyAsync(string container, IEnumerable<string> fileNames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileNames);

        if (string.IsNullOrWhiteSpace(container))
            throw new ArgumentException("Container name cannot be null or empty.", nameof(container));

        var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());

        var deleteTasks = fileNames.Select(fileName =>
            blobContainer.GetBlobClient(fileName).DeleteIfExistsAsync(cancellationToken: cancellationToken));

        await Task.WhenAll(deleteTasks);
    }

    #endregion

    #region Copy/Move Operations

    public async Task<Uri> MoveToAsync(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateParameters(fromContainer, fileName);

        if (string.IsNullOrWhiteSpace(toContainer))
            throw new ArgumentException("Destination container name cannot be null or empty.", nameof(toContainer));

        try
        {
            var uri = await CopyToAsync(fromContainer, toContainer, fileName, cancellationToken);
            await DeleteAsync(fromContainer, fileName, cancellationToken);
            return uri;
        }
        catch (StorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StorageException($"Failed to move file '{fileName}' from '{fromContainer}' to '{toContainer}'.", fromContainer, fileName, ex);
        }
    }

    public async Task<Uri> CopyToAsync(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateParameters(fromContainer, fileName);

        if (string.IsNullOrWhiteSpace(toContainer))
            throw new ArgumentException("Destination container name cannot be null or empty.", nameof(toContainer));

        try
        {
            var sourceBlobContainer = Client.GetBlobContainerClient(fromContainer.ToLowerInvariant());
            var srcBlob = sourceBlobContainer.GetBlobClient(fileName);

            if (!await srcBlob.ExistsAsync(cancellationToken))
                throw new StorageNotFoundException(fromContainer, fileName);

            var destBlobContainer = await GetOrCreateContainerAsync(toContainer, cancellationToken);
            var destBlob = destBlobContainer.GetBlobClient(fileName);

            var copyOperation = await destBlob.StartCopyFromUriAsync(srcBlob.Uri, cancellationToken: cancellationToken);
            await copyOperation.WaitForCompletionAsync(cancellationToken);

            return destBlob.Uri;
        }
        catch (StorageException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to copy file '{fileName}' from '{fromContainer}' to '{toContainer}'.", fromContainer, fileName, ex);
        }
    }

    #endregion

    #region Query Operations

    public async Task<bool> ExistsAsync(string container, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateParameters(container, fileName);

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
            var blob = blobContainer.GetBlobClient(fileName);
            return await blob.ExistsAsync(cancellationToken);
        }
        catch (RequestFailedException)
        {
            return false;
        }
    }

    public async Task<IEnumerable<StorageItem>> ListAsync(string container, CancellationToken cancellationToken = default)
    {
        return await ListAsync(container, null, cancellationToken);
    }

    public async Task<IEnumerable<StorageItem>> ListAsync(string container, string? prefix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(container))
            throw new ArgumentException("Container name cannot be null or empty.", nameof(container));

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());

            if (!await blobContainer.ExistsAsync(cancellationToken))
                return [];

            var items = new List<StorageItem>();

            await foreach (var blobItem in blobContainer.GetBlobsAsync(new GetBlobsOptions { Prefix = prefix }, cancellationToken: cancellationToken))
            {
                var blob = blobContainer.GetBlobClient(blobItem.Name);

                items.Add(new StorageItem
                {
                    Name = blobItem.Name,
                    Uri = blob.Uri,
                    ContentType = blobItem.Properties.ContentType ?? string.Empty,
                    Size = blobItem.Properties.ContentLength ?? 0,
                    LastModified = blobItem.Properties.LastModified ?? DateTimeOffset.UtcNow,
                    ETag = blobItem.Properties.ETag?.ToString(),
                    IsDirectory = false
                });
            }

            return items;
        }
        catch (RequestFailedException ex)
        {
            throw new StorageContainerException(container, $"Failed to list files in container '{container}'.", ex);
        }
    }

    public async Task<StorageMetadata> GetMetadataAsync(string container, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateParameters(container, fileName);

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
            var blob = blobContainer.GetBlobClient(fileName);

            if (!await blob.ExistsAsync(cancellationToken))
                throw new StorageNotFoundException(container, fileName);

            var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

            return new StorageMetadata
            {
                ContentType = properties.Value.ContentType,
                Size = properties.Value.ContentLength,
                LastModified = properties.Value.LastModified,
                ETag = properties.Value.ETag.ToString(),
                CustomMetadata = properties.Value.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                ContentEncoding = properties.Value.ContentEncoding,
                CacheControl = properties.Value.CacheControl
            };
        }
        catch (StorageException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to get metadata for file '{fileName}' in container '{container}'.", container, fileName, ex);
        }
    }

    public async Task SetMetadataAsync(string container, string fileName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        ValidateParameters(container, fileName);
        ArgumentNullException.ThrowIfNull(metadata);

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
            var blob = blobContainer.GetBlobClient(fileName);

            if (!await blob.ExistsAsync(cancellationToken))
                throw new StorageNotFoundException(container, fileName);

            await blob.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
        }
        catch (StorageException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to set metadata for file '{fileName}' in container '{container}'.", container, fileName, ex);
        }
    }

    #endregion

    #region URI Operations

    public Uri GetBlobUri(string container, string fileName)
    {
        ValidateParameters(container, fileName);

        var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
        var blob = blobContainer.GetBlobClient(fileName);

        return _options.EnableCdn && !string.IsNullOrWhiteSpace(_options.CdnEndpoint)
            ? new Uri($"{_options.CdnEndpoint.TrimEnd('/')}/{container.ToLowerInvariant()}/{fileName}")
            : blob.Uri;
    }

    public async Task<Uri> GetSasUriAsync(string container, string fileName, TimeSpan expiresIn, CancellationToken cancellationToken = default)
    {
        ValidateParameters(container, fileName);

        try
        {
            var blobContainer = Client.GetBlobContainerClient(container.ToLowerInvariant());
            var blob = blobContainer.GetBlobClient(fileName);

            if (!await blob.ExistsAsync(cancellationToken))
                throw new StorageNotFoundException(container, fileName);

            if (!blob.CanGenerateSasUri)
                throw new InvalidOperationException("Cannot generate SAS token. Ensure the storage account uses shared key authentication.");

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = container.ToLowerInvariant(),
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blob.GenerateSasUri(sasBuilder);
        }
        catch (StorageException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            throw new StorageException($"Failed to generate SAS URI for file '{fileName}' in container '{container}'.", container, fileName, ex);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<BlobContainerClient> GetOrCreateContainerAsync(string container, CancellationToken cancellationToken)
    {
        var containerClient = Client.GetBlobContainerClient(container.ToLowerInvariant());

        if (_options.AutoCreateContainer)
        {
            var publicAccessType = _options.PublicAccessType?.ToLowerInvariant() switch
            {
                "blob" => PublicAccessType.Blob,
                "container" => PublicAccessType.BlobContainer,
                _ => PublicAccessType.None
            };

            await containerClient.CreateIfNotExistsAsync(publicAccessType, cancellationToken: cancellationToken);
        }

        return containerClient;
    }

    private static void ValidateParameters(string container, string fileName)
    {
        if (string.IsNullOrWhiteSpace(container))
            throw new ArgumentException("Container name cannot be null or empty.", nameof(container));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
    }

    #endregion
}