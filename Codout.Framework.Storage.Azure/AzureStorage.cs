using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Codout.Framework.Storage.Azure;

public class AzureStorage : IStorage
{
    public AzureSettings Settings { get; }

    private BlobServiceClient _client;

    public BlobServiceClient Client => (_client ??= new BlobServiceClient(Settings.UseDevelopmentStorage ? "UseDevelopmentStorage=true" : $"DefaultEndpointsProtocol={Settings.Protocol};AccountName={Settings.AccountName};AccountKey={Settings.AccountKey};BlobEndpoint={Settings.Url};"));

    public AzureStorage(AzureSettings settings)
    {
        Settings = settings;
    }

    public async Task<string> Upload(Stream file, string container, string fileName)
    {
        file.Position = 0;

        var blobContainer = Client.GetBlobContainerClient(container);

        await blobContainer.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blob = blobContainer.GetBlockBlobClient(fileName);

        var blobHttpHeader = new BlobHttpHeaders
        {
            ContentType = Path.GetExtension(blob.Uri.AbsoluteUri).GetMimeType()
        };

        await blob.UploadAsync(file, blobHttpHeader);

        return GetUrl(blob.BlobContainerName, blob.Name);
    }

    public async Task<Stream> Download(string container, string fileName)
    {
        var blobContainer = Client.GetBlobContainerClient(container);
        var blob = blobContainer.GetBlockBlobClient(fileName);
        var stream = new MemoryStream();
        await blob.DownloadToAsync(stream);
        stream.Position = 0;
        return stream;
    }

    public async Task<Stream> DownloadByUrl(string container, string fileUrl)
    {
        var blobContainer = Client.GetBlobContainerClient(container);

        if (!await Exists(container, fileUrl))
            return null;

        var fileName = fileUrl.Replace(blobContainer.Uri.AbsoluteUri + "/", "");

        var blob = blobContainer.GetBlockBlobClient(fileName);
        var stream = new MemoryStream();
        await blob.DownloadToAsync(stream);
        stream.Position = 0;
        return stream;
    }

    public async Task Delete(string container, string fileName)
    {
        var blobContainer = Client.GetBlobContainerClient(container);
        var blob = blobContainer.GetBlobClient(fileName);
        await blob.DeleteIfExistsAsync();
    }

    public async Task DeleteByUrl(string container, string fileUrl)
    {
        var blobContainer = Client.GetBlobContainerClient(container);

        if (!await Exists(container, fileUrl))
            return;

        var fileName = fileUrl.Replace(blobContainer.Uri.AbsoluteUri + "/", "");

        var blob = blobContainer.GetBlobClient(fileName);

        await blob.DeleteIfExistsAsync();
    }

    public async Task<bool> Exists(string container, string fileUrl)
    {
        var blobContainer = Client.GetBlobContainerClient(container);

        var fileName = fileUrl.Replace(blobContainer.Uri.AbsoluteUri + "/", "");

        var blob = blobContainer.GetBlobClient(fileName);

        return await blob.ExistsAsync();
    }

    public string GetUrl(string container, string fileName)
    {
        if (Settings.UseDevelopmentStorage)
            return $"{Settings.Url}{container}/{fileName}";

        return $"{Client.Uri.Scheme}://{Client.Uri.Host}/{container}/{fileName}";
    }
}
