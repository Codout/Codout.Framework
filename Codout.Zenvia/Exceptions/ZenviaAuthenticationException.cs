using System;

namespace Codout.Zenvia.Exceptions
{
    /// <summary>
    /// Classe de excessão específica quando a falha está relacionada à autenticação no serviço.
    /// </summary>
    public class ZenviaAuthenticationException : ZenviaException
    {
        /// <summary>
        /// Método construtor.
        /// </summary>
        public ZenviaAuthenticationException() : base() { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public ZenviaAuthenticationException(string message) : base(message) { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaAuthenticationException(Exception innerException) : base(string.Empty, innerException) { }

        /// <summary>
        /// Método construtor
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
