using System;
using System.Collections.Generic;
using System.Linq;

namespace Codout.Framework.NetStandard.Commom.Extensions
{
    /// <summary>
    /// Extensões comuns para tipos relacionadas a listas.
    /// </summary>
    public static class Lists
    {
        #region Each
        /// <summary>
        /// Executa uma ação para cada item.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var element in enumerable)
                action(element);
        }
        #endregion

        #region Join
        /// <summary>
        /// Junta cada item de uma lista separada pelo separador.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join(this IList<string> list, string separator)
        {
            return string.Join(separator, list.ToArray());
        }
        #endregion
    }
}
