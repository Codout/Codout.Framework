using System;

namespace Codout.Framework.Common.Extensions
{
    /// <summary>
    /// Extensões comuns para tipos relacionadas a exceções.
    /// </summary>
    public static class Exceptions
    {
        #region GetMessage
        /// <summary>
        /// Retorna recursivamente todas as mensagens da excessão.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetMessage(this Exception exception)
        {
            if (exception == null)
                return string.Empty;

            if (exception.InnerException != null)
                return string.Format("{0}\r\n > {1} ",
                    exception.Message,
                    GetMessage(exception.InnerException));

            return exception.Message;
        }
        #endregion
    }
}
