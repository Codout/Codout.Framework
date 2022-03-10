using System;

namespace Codout.Zenvia.Exceptions
{
    /// <summary>
    /// Classe de excessão específica quando uma solicitação HTTP é rejeitada sob argumento de acesso negado(proibido - 403).
    /// </summary>
    public class ZenviaForbiddenException : ZenviaException
    {
        /// <summary>
        /// Método construtor.
        /// </summary>
        public ZenviaForbiddenException() : base() { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public ZenviaForbiddenException(string message) : base(message) { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaForbiddenException(Exception innerException) : base(string.Empty, innerException) { }

        /// <summary>
        /// Método construtor
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaForbiddenException(string message, Exception innerException) : base(message, innerException) { }
    }
}
