using System;

namespace Codout.Zenvia.Exceptions
{
    /// <summary>
    /// Classe de excessão específica quando uma solicitação HTTP não é concluída porque o tempo de espera esgotou(Tempo de requisição esgotado - 408).
    /// </summary>
    public class ZenviaRequestTimeoutException : ZenviaException
    {
        /// <summary>
        /// Método construtor.
        /// </summary>
        public ZenviaRequestTimeoutException() : base() { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public ZenviaRequestTimeoutException(string message) : base(message) { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaRequestTimeoutException(Exception innerException) : base(string.Empty, innerException) { }

        /// <summary>
        /// Método construtor
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaRequestTimeoutException(string message, Exception innerException) : base(message, innerException) { }
    }
}
