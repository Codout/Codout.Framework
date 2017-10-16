using System.IO;
using System.Reflection;

namespace Codout.Framework.Common.Helpers
{
    public class ResourcesHelper
    {
        public static string GetResourceContent(string baseName, Assembly assembly, string resourceName)
        {
            var stream = assembly.GetManifestResourceStream(baseName + "." + resourceName);
            var reader = new StreamReader(stream);
            string strContent = reader.ReadToEnd();
            return strContent;
        }
    }
}
