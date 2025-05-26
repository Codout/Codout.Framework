using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using HeyRed.Mime;

namespace Codout.Framework.Storage.Azure;

public class AzureStorage(AzureSettings settings) : IStorage
{
    public AzureSettings Settings { get; } = settings;

    private BlobServiceClient _client;

    public BlobServiceClient Client => (_client ??= new BlobServiceClient(Settings.UseDevelopmentStorage ? "UseDevelopmentStorage=true" : $"DefaultEndpointsProtocol={Settings.Protocol};AccountName={Settings.AccountName};AccountKey={Settings.AccountKey};BlobEndpoint={Settings.Url};"));

    public async Task<Uri> Upload(Stream file, string container, string fileName, CancellationToken cancellationToken)
    {
        file.Seek(0, SeekOrigin.Begin);

        var blobContainer = Client.GetBlobContainerClient(container.ToLower());

        await blobContainer.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blob = blobContainer.GetBlockBlobClient(fileName);

        var blobHttpHeader = new BlobHttpHeaders
        {
            ContentType = MimeTypesMap.GetMimeType(fileName)
        };

        await blob.UploadAsync(file, blobHttpHeader, cancellationToken: cancellationToken);

        return blob.Uri;
    }

    public async Task<Stream> Download(string container, string fileName, CancellationToken cancellationToken)
    {
        var blobContainer = Client.GetBlobContainerClient(container.ToLower());
        var blob = blobContainer.GetBlockBlobClient(fileName);
        var stream = new MemoryStream();
        await blob.DownloadToAsync(stream, cancellationToken: cancellationToken);
        stream.Position = 0;
        return stream;
    }

    public async Task<Stream> GetStream(string container, string fileName, CancellationToken cancellationToken)
    {
        var blobContainer = Client.GetBlobContainerClient(container.ToLower());
        var blob = blobContainer.GetBlockBlobClient(fileName);
        return await blob.OpenReadAsync(cancellationToken: cancellationToken);
    }

    public async Task Delete(string container, string fileName, CancellationToken cancellationToken)
    {
        var blobContainer = Client.GetBlobContainerClient(container.ToLower());
        var blob = blobContainer.GetBlobClient(fileName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task DeleteByUrl(string container, string fileUrl, CancellationToken cancellationToken)
    {
        var blobContainer = Client.GetBlobContainerClient(container.ToLower());

        if (!await Exists(container.ToLower(), fileUrl))
            return;

        var fileName = fileUrl.Replace(blobContainer.Uri.AbsoluteUri + "/", "");

        var blob = blobContainer.GetBlobClient(fileName);

        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> Exists(string container, string fileUrl)
    {
        var blobContainer = Client.GetBlobContainerClient(container.ToLower());

        var fileName = fileUrl.Replace(blobContainer.Uri.AbsoluteUri + "/", "");

        var blob = blobContainer.GetBlobClient(fileName);

        return await blob.ExistsAsync();
    }

    public Uri GetBlobUri(string container, string fileName)
    {
        var blobContainer = Client.GetBlobContainerClient(container.ToLower());
        var blob = blobContainer.GetBlobClient(fileName);
        return blob.Uri;
    }

    public async Task<Uri> MoveTo(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken)
    {
        var sourceBlobContainer = Client.GetBlobContainerClient(fromContainer.ToLower());

        var srcBlob = sourceBlobContainer.GetBlobClient(fileName);

        if (!await srcBlob.ExistsAsync(cancellationToken))
            throw new Exception("Source blob cannot be null.");

        var destBlobContainer = Client.GetBlobContainerClient(toContainer.ToLower());
        await destBlobContainer.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var name = srcBlob.Uri.Segments.Last();
        var destBlob = destBlobContainer.GetBlobClient(name);
        var result = await destBlob.StartCopyFromUriAsync(srcBlob.Uri, cancellationToken: cancellationToken);

        await srcBlob.DeleteAsync(cancellationToken: cancellationToken);

        return destBlob.Uri;
    }

    public async Task<Uri> CopyTo(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken)
    {
        var sourceBlobContainer = Client.GetBlobContainerClient(fromContainer.ToLower());

        var srcBlob = sourceBlobContainer.GetBlobClient(fileName);

        if (!await srcBlob.ExistsAsync(cancellationToken))
            throw new Exception("Source blob cannot be null.");

        var destBlobContainer = Client.GetBlobContainerClient(toContainer.ToLower());
        await destBlobContainer.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var name = srcBlob.Uri.Segments.Last();
        var destBlob = destBlobContainer.GetBlobClient(name);
        var result = await destBlob.StartCopyFromUriAsync(srcBlob.Uri, cancellationToken: cancellationToken);

        return destBlob.Uri;
    }
}
