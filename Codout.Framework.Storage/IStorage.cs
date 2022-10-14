using System.IO;
using System.Threading.Tasks;

namespace Codout.Framework.Storage;

public interface IStorage
{
    Task<string> Upload(Stream file, string container, string fileName);

    Task<Stream> Download(string container, string fileName);

    Task<Stream> DownloadByUrl(string container, string fileUrl);

    Task Delete(string container, string fileName);

    Task DeleteByUrl(string container, string fileUrl);

    string GetUrl(string container, string fileUrl);
}
