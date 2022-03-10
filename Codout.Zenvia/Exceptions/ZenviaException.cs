using System;

namespace Codout.Zenvia.Exceptions
{
    /// <summary>
    /// Classe de excessão geral da biblioteca.
    /// </summary>
    public class ZenviaException : Exception
    {
        /// <summary>
        /// Método construtor.
        /// </summary>
        public ZenviaException() : base() { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public ZenviaException(string message) : base(message) { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaException(Exception innerException) : base(string.Empty, innerException) { }

        /// <summary>
        /// Método construtor
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaException(string message, Exception innerException) : base(message, innerException) { }
    }
}
