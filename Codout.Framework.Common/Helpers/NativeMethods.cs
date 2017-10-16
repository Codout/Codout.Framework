using System;
using System.Runtime.InteropServices;

namespace Codout.Framework.Common.Helpers
{
    ///<summary>
    /// Métodos nativos (Windows API).
    ///</summary>
    public static class NativeMethods
    {
        #region DeleteObject
        /// <summary>
        /// Exclui um objeto GDI da memória.
        /// </summary>
        /// <param name="hObject">Ponteiro para o objeto.</param>
        /// <returns>Verdadeiro se a operação ocorreu com sucesso.</returns>
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
        #endregion
    }
}
