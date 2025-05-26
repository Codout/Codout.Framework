using System.IO;

namespace Codout.Mailer.Helpers;

public static class Extensions
{
    public static byte[] ReadFully(this Stream stream)
    {
        var buffer = new byte[32768];

        stream.Position = 0;

        using (var ms = new MemoryStream())
        {
            while (true)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) return ms.ToArray();
                ms.Write(buffer, 0, read);
            }
        }
    }
}