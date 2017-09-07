using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Codout.Framework.NetStandard.Commom.Helpers
{
    /// <summary>
    /// Classe para carregar as Dlls de uma pasta que não esteja em BIN ou na raiz da Aplicação
    /// e que implementam a interface passada no parametro T
    /// </summary>
    /// <typeparam name="T">Interface do Plugin</typeparam>
    public static class PluginLoader<T>
    {
        public static ICollection<T> LoadPlugins(string path)
        {
            if (!Directory.Exists(path))
                return new List<T>();

            var dllFileNames = Directory.GetFiles(path, "*.dll");

            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);

            foreach (var dllFile in dllFileNames)
            {
                try
                {
                    var an = AssemblyName.GetAssemblyName(dllFile);
                    var assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                }
            }

            var pluginType = typeof(T);

            ICollection<Type> pluginTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                if (assembly != null)
                {
                    var types = assembly.GetTypes();

                    foreach (var type in types)
                    {
                        if (type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }

                        if (type.GetInterface(pluginType.FullName) != null)
                        {
                            pluginTypes.Add(type);
                        }
                    }
                }
            }

            ICollection<T> plugins = new List<T>(pluginTypes.Count);
            foreach (var type in pluginTypes)
            {
                var plugin = (T)Activator.CreateInstance(type);
                plugins.Add(plugin);
            }

            return plugins;
        }
    }
}
