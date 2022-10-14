using System.IO;
using System.Threading.Tasks;

namespace Codout.Framework.Storage;

public static class StorageExtensions
{
    public static string UploadToStorage(this IStorage storage, string storagePath, string fileName)
    {
        var stream = new MemoryStream(File.ReadAllBytes(fileName)) { Position = 0 };
        var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}{Path.GetExtension(fileName)}";
        return Task.Run(async () => await storage.Upload(stream, "files", $"{storagePath}/{Path.GetFileName(newFileName)}")).Result;
    }

    public static string UploadToStorage(this IStorage storage, string storagePath, Stream stream, string fileName)
    {
        stream.Position = 0;
        var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}{Path.GetExtension(fileName)}";
        return Task.Run(async () => await storage.Upload(stream, "files", $"{storagePath}/{Path.GetFileName(newFileName)}")).Result;
    }
}
