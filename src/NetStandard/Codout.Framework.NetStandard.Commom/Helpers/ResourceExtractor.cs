﻿using System.IO;
using System.Reflection;

namespace Codout.Framework.NetStandard.Commom.Helpers
{
    /// <summary>
    /// Classe responsável por extrair 'arquivos de resources'.
    /// </summary>
    public static class ResourceExtractor
    {
        #region ExtractResourceToFile
        /// <summary>
        /// Extrai um recurso para um arquivo em disco.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="filename"></param>
        internal static void ExtractResourceToFile(string resourceName, string filename)
        {
            if (!File.Exists(filename))
                using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                using (var fs = new FileStream(filename, FileMode.Create))
                {
                    var b = new byte[s.Length];
                    s.Read(b, 0, b.Length);
                    fs.Write(b, 0, b.Length);
                }
        }
        #endregion

        #region ExtractResourceString
        /// <summary>
        /// Extrai um recurso como texto.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        internal static string ExtractResourceString(string resourceName)
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                if (s != null)
                    using (var reader = new StreamReader(s))
                        return reader.ReadToEnd();

            return null;
            //Assembly.GetExecutingAssembly().GetManifestResourceNames()
        }
        #endregion

        #region ExtractResourceToFile
        /// <summary>
        /// Extrai um recurso para um arquivo.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <param name="filename"></param>
        public static void ExtractResourceToFile(this Assembly assembly, string resourceName, string filename)
        {
            if (!File.Exists(filename))
                using (var s = assembly.GetManifestResourceStream(resourceName))
                using (var fs = new FileStream(filename, FileMode.Create))
                {
                    var b = new byte[s.Length];
                    s.Read(b, 0, b.Length);
                    fs.Write(b, 0, b.Length);
                }
        }
        #endregion

        #region ExtractResourceString
        /// <summary>
        /// Extrai um recurso como texto.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static string ExtractResourceString(this Assembly assembly, string resourceName)
        {
            using (var s = assembly.GetManifestResourceStream(resourceName))
                if (s != null)
                    using (var reader = new StreamReader(s))
                        return reader.ReadToEnd();

            return null;
        }
        #endregion
    }
}