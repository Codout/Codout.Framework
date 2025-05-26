using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Codout.Framework.Storage;

public interface IStorage
{
    Task<Uri> Upload(Stream file, string container, string fileName, CancellationToken cancellationToken);

    Task<Stream> Download(string container, string fileName, CancellationToken cancellationToken);

    Task<Stream> GetStream(string container, string fileName, CancellationToken cancellationToken);

    Task Delete(string container, string fileName, CancellationToken cancellationToken);

    Task<Uri> MoveTo(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken);

    Task<Uri> CopyTo(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken);

    Uri GetBlobUri(string container, string fileName);
}